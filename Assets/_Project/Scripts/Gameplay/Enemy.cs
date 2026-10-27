using System.Collections.Generic;
using UnityEngine;

namespace PayWithPixels
{
    public enum EnemyType { Chaser, Shooter, Tank, Boss, Collector }

    public class Enemy : MonoBehaviour
    {
        public static readonly List<Enemy> All = new List<Enemy>();

        public float radius;
        private EnemyType type;
        private int hp;
        private float speed;
        private int contactDamage = 1;
        private float shootTimer;
        private float strafeSign = 1f;
        private int maxHp;
        private float clock;
        private int pattern;
        private SpriteRenderer sr;
        private string walkName;
        private int spriteSize;
        private bool diamond;

        public static Enemy SpawnCollector(Transform parent, Vector2 pos)
        {
            var go = new GameObject("Collector");
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            var e = go.AddComponent<Enemy>();
            e.type = EnemyType.Collector;
            e.sr = Art.MakeRenderer(go, Art.Square("enemy_collector_walk_0", 20), 6);
            e.walkName = "enemy_collector"; e.spriteSize = 20;
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

        public static Enemy Spawn(Transform parent, EnemyType type, Vector2 pos, int wave)
        {
            var go = new GameObject(type.ToString());
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            var e = go.AddComponent<Enemy>();
            e.type = type;
            float scale = 1f + 0.03f * wave;

            switch (type)
            {
                case EnemyType.Chaser:
                    e.sr = Art.MakeRenderer(go, Art.Diamond("enemy_chaser", 16), 5);
                    e.walkName = "enemy_chaser"; e.spriteSize = 16; e.diamond = true;
                    e.radius = 0.45f; e.hp = 1 + wave / 4; e.speed = 2.6f * scale;
                    break;
                case EnemyType.Shooter:
                    e.sr = Art.MakeRenderer(go, Art.Square("enemy_shooter", 16), 5);
                    e.walkName = "enemy_shooter"; e.spriteSize = 16;
                    e.radius = 0.45f; e.hp = 2 + wave / 4; e.speed = 2f * scale;
                    e.shootTimer = Random.Range(0.5f, 1.8f);
                    e.strafeSign = Random.value < 0.5f ? -1f : 1f;
                    break;
                default:
                    e.sr = Art.MakeRenderer(go, Art.Square("enemy_tank", 24), 5);
                    e.walkName = "enemy_tank"; e.spriteSize = 24;
                    e.radius = 0.7f; e.hp = 8 + wave; e.speed = 1.2f * scale; e.contactDamage = 2;
                    break;
            }
            return e;
        }

        private void OnEnable() => All.Add(this);
        private void OnDisable() => All.Remove(this);

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
            if (type == EnemyType.Collector)
            {
                Game.Instance.OnCollectorKilled();
                Game.Instance.SpawnCoin(transform.position, 6);
                Destroy(gameObject);
                return;
            }
            Game.Instance.SpawnCoin(transform.position, type == EnemyType.Tank ? 3 : 1);
            Destroy(gameObject);
        }

        private void Update()
        {
            var game = Game.Instance;
            if (game.state != Game.State.Playing && game.state != Game.State.Clearing) return;
            if (game.player == null) return;

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

            if (type == EnemyType.Shooter)
            {
                if (dist > 6f) move = dir;
                else if (dist < 4f) move = -dir;
                else move = new Vector2(-dir.y, dir.x) * strafeSign;

                shootTimer -= Time.deltaTime;
                if (shootTimer <= 0 && game.state == Game.State.Playing)
                {
                    shootTimer = 1.8f;
                    Bullet.Spawn(transform.parent, pos + dir * 0.5f, dir * 5f, false, 0);
                }
            }
            else move = dir;

            clock += Time.deltaTime;
            if (walkName != null)
            {
                string frame = walkName + "_walk_" + (Mathf.FloorToInt(clock / 0.25f) % 2);
                sr.sprite = diamond ? Art.Diamond(frame, spriteSize) : Art.Square(frame, spriteSize);
            }

            pos += move * (speed * Time.deltaTime);
            pos.x = Mathf.Clamp(pos.x, -Game.ArenaHalf.x, Game.ArenaHalf.x);
            pos.y = Mathf.Clamp(pos.y, -Game.ArenaHalf.y, Game.ArenaHalf.y);
            transform.position = pos;
            if (dir != Vector2.zero) transform.rotation = Art.FaceRotation(dir);

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
