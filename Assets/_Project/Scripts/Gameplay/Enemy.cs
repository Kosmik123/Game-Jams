using System.Collections.Generic;
using UnityEngine;

namespace PayWithPixels
{
    public enum EnemyType { Chaser, Shooter, Tank, Dasher, Siege, Swarm, Guard, Spawner, Boss, Collector }

    public class Enemy : MonoBehaviour
    {
        public static readonly List<Enemy> All = new List<Enemy>();

        public float radius;
        public bool IsRegular => type != EnemyType.Boss && type != EnemyType.Collector;
        private EnemyType type;
        private int hp;
        private int maxHp;
        private float speed;
        private int contactDamage = 1;
        private int coinDrop = 1;
        private float shootTimer;
        private float strafeSign = 1f;
        private float clock;
        private int pattern;
        private SpriteRenderer sr;
        private string walkName;
        private int spriteSize;
        private bool rotates;

        // Skaczek
        private float dashTimer;
        private float telegraph;
        private Vector2 dashDir;
        private float dashLeft;
        // Straznik
        private Vector2 facing = Vector2.down;
        // Spawner
        private int spawned;

        private struct Spec
        {
            public string sprite; public int size; public float radius; public int hp; public float speed; public int contact; public int coins; public bool rotates;
        }

        private static Spec SpecFor(EnemyType t, int d)
        {
            switch (t)
            {
                case EnemyType.Chaser: return new Spec { sprite = "enemy_chaser", size = 16, radius = 0.45f, hp = 1 + d / 4, speed = 2.6f, contact = 1, coins = 1, rotates = true };
                case EnemyType.Shooter: return new Spec { sprite = "enemy_shooter", size = 16, radius = 0.45f, hp = 2 + d / 4, speed = 2f, contact = 1, coins = 1, rotates = true };
                case EnemyType.Tank: return new Spec { sprite = "enemy_tank", size = 24, radius = 0.7f, hp = 8 + d, speed = 1.2f, contact = 2, coins = 3, rotates = true };
                case EnemyType.Dasher: return new Spec { sprite = "enemy_dasher", size = 16, radius = 0.45f, hp = 2 + d / 5, speed = 2.2f, contact = 1, coins = 2, rotates = true };
                case EnemyType.Siege: return new Spec { sprite = "enemy_siege", size = 16, radius = 0.5f, hp = 4 + d / 4, speed = 1.6f, contact = 1, coins = 3, rotates = false };
                case EnemyType.Swarm: return new Spec { sprite = "enemy_swarm", size = 6, radius = 0.3f, hp = 1, speed = 3.3f, contact = 1, coins = 0, rotates = false };
                case EnemyType.Guard: return new Spec { sprite = "enemy_guard", size = 16, radius = 0.5f, hp = 5 + d / 3, speed = 1.7f, contact = 1, coins = 3, rotates = true };
                default: return new Spec { sprite = "enemy_spawner", size = 24, radius = 0.7f, hp = 8 + d / 2, speed = 0.7f, contact = 1, coins = 4, rotates = false };
            }
        }

        public static Enemy Spawn(Transform parent, EnemyType type, Vector2 pos, int difficulty)
        {
            var go = new GameObject(type.ToString());
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            var e = go.AddComponent<Enemy>();
            e.type = type;

            var s = SpecFor(type, difficulty);
            float scale = 1f + 0.03f * difficulty;
            e.walkName = s.sprite; e.spriteSize = s.size; e.rotates = s.rotates;
            e.sr = Art.MakeRenderer(go, Art.Square(s.sprite + "_walk_0", s.size), 5);
            e.radius = s.radius; e.hp = e.maxHp = s.hp; e.speed = s.speed * scale;
            e.contactDamage = s.contact; e.coinDrop = s.coins;

            e.shootTimer = Random.Range(0.5f, 1.8f);
            e.strafeSign = Random.value < 0.5f ? -1f : 1f;
            e.dashTimer = Random.Range(1f, 2.4f);
            if (type == EnemyType.Siege) e.shootTimer = Random.Range(1.5f, 3f);
            if (type == EnemyType.Spawner) e.shootTimer = 3f;
            return e;
        }

        public static Enemy SpawnCollector(Transform parent, Vector2 pos)
        {
            var go = new GameObject("Collector");
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            var e = go.AddComponent<Enemy>();
            e.type = EnemyType.Collector;
            e.sr = Art.MakeRenderer(go, Art.Square("enemy_collector_walk_0", 24), 6);
            e.walkName = "enemy_collector"; e.spriteSize = 24;
            e.radius = 0.6f;
            e.hp = e.maxHp = 25 + 3 * Game.Instance.zone;
            e.speed = 3.1f;
            e.contactDamage = 0;
            return e;
        }

        public static Enemy SpawnBoss(Transform parent, Vector2 pos, int zone)
        {
            var go = new GameObject("Boss");
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            var e = go.AddComponent<Enemy>();
            e.type = EnemyType.Boss;
            e.sr = Art.MakeRenderer(go, Art.Square("boss_idle_0", 48), 6);
            e.radius = 1.4f;
            e.hp = e.maxHp = 60 + 25 * (zone - 1);
            e.contactDamage = 2;
            e.shootTimer = 2f;
            return e;
        }

        private void OnEnable() => All.Add(this);
        private void OnDisable() => All.Remove(this);

        /// <summary>Straznik: pocisk lecacy z przodu (od strony gracza) jest blokowany.</summary>
        public bool Blocks(Vector2 bulletVelocity)
        {
            if (type != EnemyType.Guard) return false;
            return Vector2.Dot(-bulletVelocity.normalized, facing) > 0.45f;
        }

        public void Damage(int dmg)
        {
            hp -= dmg;
            if (hp > 0) { Sfx.Play(Sound.Hit); return; }
            Sfx.Play(Sound.Kill);
            if (type == EnemyType.Boss)
            {
                Game.Instance.OnBossKilled();
                return;
            }
            Shard.Burst(transform.parent, transform.position, 5);
            if (type != EnemyType.Collector) Game.Instance.OnEnemyKilled(transform.position);
            if (type == EnemyType.Collector)
            {
                Game.Instance.OnCollectorKilled();
                Game.Instance.SpawnCoin(transform.position, 6);
                Destroy(gameObject);
                return;
            }
            if (coinDrop > 0) Game.Instance.SpawnCoin(transform.position, coinDrop);
            else if (Random.value < 0.5f) Game.Instance.SpawnCoin(transform.position, 1);
            Destroy(gameObject);
        }

        private void UpdateBoss(Game game)
        {
            clock += Time.deltaTime;
            sr.sprite = Art.Square(Mathf.FloorToInt(clock / 0.4f) % 2 == 0 ? "boss_idle_0" : "boss_idle_1", 48);

            // wolno patroluje przy gornej krawedzi
            var pos = new Vector2(Mathf.Sin(clock * 0.6f) * (Game.ArenaHalf.x - 2f), Game.ArenaHalf.y - 1.6f);
            transform.position = pos;

            shootTimer -= Time.deltaTime;
            if (shootTimer > 0) return;
            bool enraged = hp * 2 < maxHp;
            shootTimer = enraged ? 1.1f : 1.7f;
            pattern++;

            Vector2 toPlayer = (Vector2)game.player.transform.position - pos;
            float baseAngle = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;
            if (pattern % 2 == 0)
            {
                // pierscien
                int n = enraged ? 18 : 12;
                float offset = Random.Range(0f, 360f);
                for (int i = 0; i < n; i++)
                    FireBoss(pos, offset + i * 360f / n, 4f);
            }
            else
            {
                // wachlarz w gracza
                int n = enraged ? 7 : 5;
                for (int i = 0; i < n; i++)
                    FireBoss(pos, baseAngle + (i - (n - 1) / 2f) * 12f, 6f);
            }
        }

        private void FireBoss(Vector2 pos, float angleDeg, float speed)
        {
            var d = new Vector2(Mathf.Cos(angleDeg * Mathf.Deg2Rad), Mathf.Sin(angleDeg * Mathf.Deg2Rad));
            Bullet.Spawn(transform.parent, pos + d * 1.2f, d * speed, false, 0, "bullet_boss");
        }

        private void Update()
        {
            var game = Game.Instance;
            if (game.state != Game.State.Playing && game.state != Game.State.Clearing) return;
            if (game.player == null || game.Frozen) return;

            if (type == EnemyType.Boss)
            {
                UpdateBoss(game);
                if (((Vector2)game.player.transform.position - (Vector2)transform.position).magnitude < radius + Player.Radius)
                    game.player.Hurt(contactDamage);
                return;
            }

            Vector2 pos = transform.position;
            Vector2 toPlayer = (Vector2)game.player.transform.position - pos;
            float dist = toPlayer.magnitude;
            Vector2 dir = dist > 0.001f ? toPlayer / dist : Vector2.zero;
            Vector2 move = Vector2.zero;
            float moveSpeed = speed;
            if (game.stats.slowAura > 0 && dist < game.stats.SlowAuraRadius && type != EnemyType.Boss)
                moveSpeed *= game.stats.SlowAuraFactor;   // zysk Spowolnienie
            bool attacking = game.state == Game.State.Playing;
            float dt = Time.deltaTime;

            switch (type)
            {
                case EnemyType.Shooter:
                    if (dist > 6f) move = dir;
                    else if (dist < 4f) move = -dir;
                    else move = new Vector2(-dir.y, dir.x) * strafeSign;
                    shootTimer -= dt;
                    if (shootTimer <= 0 && attacking)
                    {
                        shootTimer = 1.8f;
                        Bullet.Spawn(transform.parent, pos + dir * 0.5f, dir * 5f, false, 0);
                    }
                    break;

                case EnemyType.Dasher:
                    if (dashLeft > 0)
                    {
                        dashLeft -= dt;
                        move = dashDir;
                        moveSpeed = 13f;
                        if (dashLeft <= 0) dashTimer = 2.2f;
                    }
                    else if (telegraph > 0)
                    {
                        telegraph -= dt;
                        sr.enabled = Mathf.FloorToInt(telegraph * 20f) % 2 == 0;   // miga przed skokiem
                        if (telegraph <= 0) { sr.enabled = true; dashLeft = 0.32f; dashDir = dir; }
                    }
                    else
                    {
                        move = dir;
                        dashTimer -= dt;
                        if (dashTimer <= 0 && attacking && dist < 8f) { telegraph = 0.55f; move = Vector2.zero; }
                    }
                    break;

                case EnemyType.Siege:
                    if (dist < 6f) move = -dir;
                    else if (dist > 9f) move = dir;
                    shootTimer -= dt;
                    if (shootTimer <= 0 && attacking)
                    {
                        shootTimer = 3.6f;
                        AreaBlast.Spawn(transform.parent, game.player.transform.position, 1.3f, 1.7f, 2);
                    }
                    break;

                case EnemyType.Spawner:
                    move = dist < 5f ? -dir : Vector2.zero;
                    shootTimer -= dt;
                    if (shootTimer <= 0 && attacking && spawned < 8)
                    {
                        shootTimer = 4f;
                        spawned++;
                        Spawn(transform.parent, EnemyType.Chaser, pos, game.Difficulty);
                    }
                    break;

                case EnemyType.Guard:
                    move = dir;
                    facing = dir == Vector2.zero ? facing : dir;
                    break;

                default:
                    move = dir;
                    break;
            }

            clock += dt;
            if (walkName != null && sr != null)
                sr.sprite = Art.Square(walkName + "_walk_" + (Mathf.FloorToInt(clock / 0.25f) % 2), spriteSize);

            pos += move * (moveSpeed * dt);
            pos.x = Mathf.Clamp(pos.x, -Game.ArenaHalf.x, Game.ArenaHalf.x);
            pos.y = Mathf.Clamp(pos.y, -Game.ArenaHalf.y, Game.ArenaHalf.y);
            pos = Obstacles.Push(pos, radius * 0.8f);
            transform.position = pos;
            if (rotates && dir != Vector2.zero && dashLeft <= 0) transform.rotation = Art.FaceRotation(dir);

            float r = radius + Player.Radius;
            if (dist < r)
            {
                if (type == EnemyType.Collector)
                {
                    game.OnCollectorTouch();
                    Destroy(gameObject);
                    return;
                }
                game.player.Hurt(contactDamage);
            }
        }
    }
}
