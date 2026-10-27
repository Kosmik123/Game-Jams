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
        public EnemyType type { get; private set; }
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
        private float flash;

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
                case EnemyType.Swarm: return new Spec { sprite = "enemy_swarm", size = 6, radius = 0.3f, hp = 1, speed = 2.8f, contact = 1, coins = 0, rotates = false };
                case EnemyType.Guard: return new Spec { sprite = "enemy_guard", size = 16, radius = 0.5f, hp = 5 + d / 3, speed = 1.7f, contact = 1, coins = 3, rotates = true };
                default: return new Spec { sprite = "enemy_spawner", size = 24, radius = 0.7f, hp = 8 + d / 2, speed = 0.7f, contact = 1, coins = 4, rotates = false };
            }
        }

        /// <summary>Limit predkosci zwyklych wrogow: 0.8 predkosci gracza z 1 stackiem nog (5 * 0.92); rojowi nizej, bo goni tlumem.</summary>
        public static float MaxSpeed(EnemyType t) => t == EnemyType.Swarm ? 2.7f :0.8f * 5f * 0.92f;

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
            e.radius = s.radius; e.hp = e.maxHp = Mathf.CeilToInt(s.hp * Game.Instance.EnemyHpMul); e.speed = Mathf.Min(s.speed * scale * Game.Instance.EnemySpeedMul, MaxSpeed(type));
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
            e.hp = e.maxHp = 35 + 6 * Game.Instance.zone;
            e.speed = 3.5f;
            e.contactDamage = 0;
            return e;
        }

        private int bossZone = 1;
        private int phase;
        private float spawnAddsTimer;
        private float auctionTimer;
        private float spin;
        private string bossSprite = "boss_idle";
        private string bossBullet = "bullet_boss";

        public static Enemy SpawnBoss(Transform parent, Vector2 pos, int zone)
        {
            var go = new GameObject("Boss");
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            var e = go.AddComponent<Enemy>();
            e.type = EnemyType.Boss;
            e.bossZone = zone;
            e.bossSprite = zone == 1 ? "boss_idle" : zone == 2 ? "boss2_idle" : "boss3_idle";
            e.bossBullet = zone == 1 ? "bullet_boss" : zone == 2 ? "bullet_boss2" : "bullet_boss3";
            e.sr = Art.MakeRenderer(go, Art.Square(e.bossSprite + "_0", 48), 6);
            e.radius = 1.4f;
            e.hp = e.maxHp = Mathf.CeilToInt((zone == 1 ? 50 : zone == 2 ? 95 : 130) * Game.Instance.BossHpMul);
            e.contactDamage = 2;
            e.shootTimer = 2f;
            e.spawnAddsTimer = 6f;
            e.auctionTimer = 8f;
            return e;
        }

        private static readonly Dictionary<long, List<Enemy>> grid = new Dictionary<long, List<Enemy>>();
        private static readonly Stack<List<Enemy>> cellPool = new Stack<List<Enemy>>();

        /// <summary>
        /// Miekkie rozpychanie: zwykli wrogowie odpychaja sasiadow, gdy sa blizej niz 0.75 sumy promieni
        /// (sila proporcjonalna do zakladki, bez fizyki). Siatka przestrzenna 1.5 j. - tanie przy 60+ wrogach.
        /// </summary>
        public static void Separate(float dt)
        {
            foreach (var kv in grid) { kv.Value.Clear(); cellPool.Push(kv.Value); }
            grid.Clear();
            for (int i = 0; i < All.Count; i++)
            {
                var e = All[i];
                if (!e.IsRegular) continue;
                Vector2 p = e.transform.position;
                long key = Key(Mathf.FloorToInt(p.x / 1.5f), Mathf.FloorToInt(p.y / 1.5f));
                if (!grid.TryGetValue(key, out var cell)) grid[key] = cell = cellPool.Count > 0 ? cellPool.Pop() : new List<Enemy>();
                cell.Add(e);
            }
            float k = Mathf.Min(1f, 10f * dt);
            for (int i = 0; i < All.Count; i++)
            {
                var a = All[i];
                if (!a.IsRegular) continue;
                Vector2 pa = a.transform.position;
                int cx = Mathf.FloorToInt(pa.x / 1.5f), cy = Mathf.FloorToInt(pa.y / 1.5f);
                Vector2 push = Vector2.zero;
                for (int dx = -1; dx <= 1; dx++)
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (!grid.TryGetValue(Key(cx + dx, cy + dy), out var cell)) continue;
                        foreach (var b in cell)
                        {
                            if (b == a) continue;
                            Vector2 d = pa - (Vector2)b.transform.position;
                            float min = 0.75f * (a.radius + b.radius);
                            float sq = d.sqrMagnitude;
                            if (sq >= min * min) continue;
                            float dist = Mathf.Sqrt(sq);
                            Vector2 n = dist > 0.001f ? d / dist : new Vector2(Mathf.Cos(i), Mathf.Sin(i));
                            push += n * ((min - dist) * 0.5f);   // kazdy z pary przesuwa sie o polowe zakladki
                        }
                    }
                if (push.sqrMagnitude < 1e-8f) continue;
                Vector2 np = pa + push * k;
                if (!Obstacles.HitsSolid(np, a.radius * 0.8f)) a.transform.position = np;
            }
        }

        private static long Key(int x, int y) => ((long)x << 32) ^ (uint)y;

        private void OnEnable() => All.Add(this);
        private void OnDisable() => All.Remove(this);

        /// <summary>Straznik: pocisk lecacy z przodu (od strony gracza) jest blokowany.</summary>
        public bool Blocks(Vector2 bulletVelocity)
        {
            if (type != EnemyType.Guard) return false;
            return Vector2.Dot(-bulletVelocity.normalized, facing) > 0.45f;
        }

        public void Damage(int dmg, bool crit = false)
        {
            hp -= dmg;
            if (hp > 0) { Sfx.Play(Sound.Hit); flash = 0.07f; return; }
            Sfx.Play(Sound.Kill);
            if (type == EnemyType.Boss)
            {
                Game.Instance.OnBossKilled();
                return;
            }
            Shard.Burst(transform.parent, transform.position, 5);
            if (crit)
            {
                var st = Game.Instance.stats;
                if (st.critBlast > 0) Explosion.Spawn(transform.parent, transform.position, 1.6f, 2);
                if (st.critCoins > 0) Game.Instance.SpawnCoin(transform.position, st.critCoins);
            }
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
            sr.sprite = Art.Square(bossSprite + "_" + (Mathf.FloorToInt(clock / 0.4f) % 2), 48);
            sr.enabled = shootTimer > 0.25f || Mathf.FloorToInt(shootTimer * 24f) % 2 == 0;   // telegraf przed salwa
            switch (bossZone)
            {
                case 1: UpdateTollKeeper(game); break;
                case 2: UpdateBanker(game); break;
                default: UpdateAuctioneer(game); break;
            }
        }

        private Vector2 AimAngleDir(float deg) => new Vector2(Mathf.Cos(deg * Mathf.Deg2Rad), Mathf.Sin(deg * Mathf.Deg2Rad));

        private float AngleToPlayer(Game game, Vector2 from)
        {
            Vector2 d = (Vector2)game.player.transform.position - from;
            return Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
        }

        /// <summary>Strefa 1: Toll Keeper - pierscienie i wachlarze.</summary>
        private void UpdateTollKeeper(Game game)
        {
            var pos = new Vector2(Mathf.Sin(clock * 0.6f) * (Game.ArenaHalf.x - 2f), Game.ArenaHalf.y - 1.6f);
            transform.position = pos;

            shootTimer -= Time.deltaTime;
            if (shootTimer > 0) return;
            bool enraged = hp * 2 < maxHp;
            shootTimer = enraged ? 1.3f : 2.0f;
            pattern++;

            float baseAngle = AngleToPlayer(game, pos);
            if (pattern % 2 == 0)
            {
                int n = enraged ? 14 : 10;
                float offset = Random.Range(0f, 360f);
                for (int i = 0; i < n; i++) FireBoss(pos, offset + i * 360f / n, 4f);
            }
            else
            {
                int n = enraged ? 7 : 5;
                for (int i = 0; i < n; i++) FireBoss(pos, baseAngle + (i - (n - 1) / 2f) * 12f, 6f);
            }
        }

        /// <summary>Strefa 2: The Banker - 3 fazy: deszcz monet, poscig z wachlarzem, zamkniety skarbiec ze spirala.</summary>
        private void UpdateBanker(Game game)
        {
            int newPhase = hp * 3 > maxHp * 2 ? 1 : hp * 3 > maxHp ? 2 : 3;
            if (newPhase != phase)
            {
                phase = newPhase;
                shootTimer = 1f;
                spawnAddsTimer = 4f;
                if (phase == 2) game.Announce("THE BANKER CALLS IN YOUR LOAN");
                if (phase == 3) game.Announce("THE VAULT IS SEALED");
            }

            Vector2 pos = transform.position;
            float topY = Game.ArenaHalf.y - 1.6f;
            if (phase == 1) pos = new Vector2(Mathf.Sin(clock * 0.5f) * (Game.ArenaHalf.x - 2f), topY);
            else if (phase == 2)
            {
                float px = game.player.transform.position.x;
                pos.x = Mathf.MoveTowards(pos.x, Mathf.Clamp(px, -Game.ArenaHalf.x + 2f, Game.ArenaHalf.x - 2f), 2.6f * Time.deltaTime);
                pos.y = topY;
            }
            else pos = Vector2.MoveTowards(pos, new Vector2(0, 1.8f), 3f * Time.deltaTime);
            transform.position = pos;

            shootTimer -= Time.deltaTime;
            if (shootTimer <= 0)
            {
                if (phase == 1)
                {
                    shootTimer = 1.5f;
                    // deszcz monet: kolumny pociskow spadajacych w dol z losowych miejsc
                    for (int i = 0; i < 7; i++)
                    {
                        float x = Random.Range(-Game.ArenaHalf.x, Game.ArenaHalf.x);
                        Bullet.Spawn(transform.parent, new Vector2(x, Game.ArenaHalf.y + 0.3f), new Vector2(0, -4.5f), false, 0, bossBullet);
                    }
                }
                else if (phase == 2)
                {
                    shootTimer = 1.3f;
                    float a = AngleToPlayer(game, pos);
                    for (int i = 0; i < 5; i++) FireBoss(pos, a + (i - 2) * 11f, 6f);
                }
                else
                {
                    shootTimer = 0.55f;
                    spin += 23f;
                    for (int i = 0; i < 6; i++) FireBoss(pos, spin + i * 60f, 4.2f);
                }
            }

            if (phase >= 2)
            {
                spawnAddsTimer -= Time.deltaTime;
                if (spawnAddsTimer <= 0)
                {
                    spawnAddsTimer = phase == 2 ? 7f : 8f;
                    for (int i = 0; i < 2; i++) Spawn(transform.parent, EnemyType.Chaser, pos + Random.insideUnitCircle, game.Difficulty);
                }
            }
        }

        /// <summary>Strefa 3: The Auctioneer - sprzedaje Twoje zyski podczas walki (wracaja po czasie albo po jego smierci).</summary>
        private void UpdateAuctioneer(Game game)
        {
            Vector2 pos = new Vector2(Mathf.Sin(clock * 0.45f) * (Game.ArenaHalf.x - 2.5f), 2.2f + Mathf.Sin(clock * 0.9f) * 1.6f);
            transform.position = pos;

            int stage = hp * 4 > maxHp * 3 ? 0 : hp * 2 > maxHp ? 1 : hp * 4 > maxHp ? 2 : 3;
            if (stage != phase)
            {
                phase = stage;
                if (phase > 0) auctionTimer = 0.5f;   // kazdy prog HP = licytacja
            }

            auctionTimer -= Time.deltaTime;
            if (auctionTimer <= 0)
            {
                auctionTimer = 22f;
                game.AuctionOffGain(14f);
            }

            shootTimer -= Time.deltaTime;
            if (shootTimer > 0) return;
            pattern++;
            shootTimer = phase >= 2 ? 1.0f : 1.35f;
            float a = AngleToPlayer(game, pos);
            if (pattern % 3 == 0)
            {
                int n = phase >= 2 ? 18 : 12;
                float offset = Random.Range(0f, 360f);
                for (int i = 0; i < n; i++) FireBoss(pos, offset + i * 360f / n, 4.2f);
            }
            else
            {
                int n = phase >= 1 ? 4 : 3;
                for (int i = 0; i < n; i++) FireBoss(pos, a + (i - (n - 1) / 2f) * 9f, 7.5f);
            }
        }

        private void FireBoss(Vector2 pos, float angleDeg, float speed)
        {
            var d = AimAngleDir(angleDeg);
            Bullet.Spawn(transform.parent, pos + d * 1.2f, d * speed, false, 0, bossBullet);
        }

        private void Update()
        {
            var game = Game.Instance;
            if (flash > 0)
            {
                flash -= Time.deltaTime;
                if (sr != null) sr.color = flash > 0 ? Color.black : Color.white;   // blysk: sprite w kolorze tla
            }
            if (game.state != Game.State.Playing && game.state != Game.State.Clearing) return;
            if (game.Frozen && sr != null) sr.enabled = true;
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
                    sr.enabled = shootTimer > 0.3f || Mathf.FloorToInt(shootTimer * 20f) % 2 == 0;   // telegraf: miga przed strzalem
                    if (shootTimer <= 0 && attacking)
                    {
                        shootTimer = 1.8f;
                        sr.enabled = true;
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
                    if (dir != Vector2.zero) facing = ((Vector2)Vector3.RotateTowards(facing, dir, 1.6f * dt, 0f)).normalized;   // tarcza obraca sie powoli: da sie go okrazyc
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
            if (dist < r * 0.85f && type != EnemyType.Collector)   // miekkie wypychanie: wrog nie zostaje "w srodku" gracza
                transform.position = (Vector2)transform.position - (dir == Vector2.zero ? Vector2.right : dir) * ((r * 0.85f - dist) * Mathf.Min(1f, 10f * dt));
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
