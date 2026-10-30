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
        private Transform barRoot, barFill;
        private float barWidth;
        private static Sprite pixelMid, pixelLeft;
        private static bool guardHintShown;
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
        private bool fleeing;                // Poborca po dotknieciu gracza ucieka za brzeg areny
        private string heldGain;             // zabrany zysk (wraca, jesli Poborca zginie w ucieczce)
        private int heldLevel;
        private SpriteRenderer shieldSr;   // Straznik: tarcza jako osobny sprite, niezalezny od obrotu korpusu (kierunek ruchu)
        private int shieldDir = -1;
        // Spawner
        private int spawned;
        private float flash;

        private struct Spec
        {
            public string sprite; public int size; public float radius; public int hpBase; public float speed; public int contact; public int coins; public bool rotates;
        }

        private static Spec SpecFor(EnemyType t, float d)
        {
            switch (t)
            {
                case EnemyType.Chaser: return new Spec { sprite = "enemy_chaser", size = 16, radius = 0.45f, hpBase = Mathf.FloorToInt(1 + d / 3), speed = 2.6f, contact = 1, coins = 1, rotates = true };
                case EnemyType.Shooter: return new Spec { sprite = "enemy_shooter", size = 16, radius = 0.45f, hpBase = Mathf.FloorToInt(2 + d / 4), speed = 2f, contact = 1, coins = 1, rotates = true };
                case EnemyType.Tank: return new Spec { sprite = "enemy_tank", size = 24, radius = 0.7f, hpBase = Mathf.FloorToInt(6 + d * 3 / 4), speed = 1.2f, contact = 2, coins = 3, rotates = true };
                case EnemyType.Dasher: return new Spec { sprite = "enemy_dasher", size = 16, radius = 0.45f, hpBase = Mathf.FloorToInt(2 + d / 5), speed = 2.2f, contact = 1, coins = 2, rotates = true };
                case EnemyType.Siege: return new Spec { sprite = "enemy_siege", size = 16, radius = 0.5f, hpBase = Mathf.FloorToInt(4 + d / 4), speed = 1.6f, contact = 1, coins = 3, rotates = false };
                case EnemyType.Swarm: return new Spec { sprite = "enemy_swarm", size = 6, radius = 0.3f, hpBase = 1, speed = 2.8f, contact = 1, coins = 0, rotates = false };
                case EnemyType.Guard: return new Spec { sprite = "enemy_guard", size = 16, radius = 0.5f, hpBase = Mathf.FloorToInt(4 + d / 4), speed = 1.7f, contact = 1, coins = 3, rotates = true };
                default: return new Spec { sprite = "enemy_spawner", size = 24, radius = 0.7f, hpBase = Mathf.FloorToInt(8 + d / 2), speed = 0.7f, contact = 1, coins = 3, rotates = false };
            }
        }

        /// <summary>Limit predkosci zwyklych wrogow: 0.8 predkosci gracza z 1 stackiem nog (5 * 0.92); rojowi nizej, bo goni tlumem.</summary>
        public static float MaxSpeed(EnemyType t) => t == EnemyType.Swarm ? 2.7f :0.8f * 5f * 0.92f;

        public static Enemy Spawn(Transform parent, EnemyType type, Vector2 pos, float difficulty, int? overrideCoins = null)
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
            e.radius = s.radius; e.hp = e.maxHp = Mathf.CeilToInt(s.hpBase * Game.Instance.EnemyHpMul); e.speed = Mathf.Min(s.speed * scale * Game.Instance.EnemySpeedMul, MaxSpeed(type));
            e.contactDamage = s.contact;
            e.coinDrop = overrideCoins.HasValue ? overrideCoins.Value : s.coins;
            if (e.maxHp >= 8 && type != EnemyType.Guard) e.CreateBar();
            if (type == EnemyType.Guard)
            {
                var sh = new GameObject("Shield");
                sh.transform.SetParent(go.transform, false);
                e.shieldSr = Art.MakeRenderer(sh, Art.ShieldArc(6), 6);
                e.UpdateShield();
            }
            if (type == EnemyType.Guard && !guardHintShown)
            {
                guardHintShown = true;
                Game.Instance.Announce("GUARD: SHOOT FROM THE SIDE");
            }

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
            e.walkName = "enemy_collector";
            e.spriteSize = 24;
            e.radius = 0.6f;
            // kazde kolejne spotkanie w runie: +12% HP bazowego
            e.hp = e.maxHp = Mathf.RoundToInt((30 + 6 * Game.Instance.zone) * (1f + 0.12f * Game.Instance.collectorMeetings) + Game.Instance.debt + Game.Instance.Difficulty);
            Game.Instance.collectorMeetings++;
            e.speed = 3.5f;
            e.CreateBar();
            e.contactDamage = 0;
            e.coinDrop = 1;
            return e;
        }

        private int bossZone = 1;
        private int phase;
        private int tollPhase = 1;
        private float gapAngle, lineTelegraph, lineGap;
        private bool lineVertical;
        private int lineSide = 1;
        private float wallTelegraph, wallGap;
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
        /// Miekkie rozpychanie: zwykli wrogowie odpychaja sasiadow, gdy sa blizej niz 1.5 sumy promieni
        /// (sila proporcjonalna do zakladki, bez fizyki). Siatka przestrzenna 2.5 j. (>= 1.5 * najwieksza suma promieni 1.4) - tanie przy 60+ wrogach.
        /// </summary>
        private const float SeparationCell = 2.5f, SeparationFactor = 1.5f;

        public static void Separate(float dt)
        {
            foreach (var kv in grid) { kv.Value.Clear(); cellPool.Push(kv.Value); }
            grid.Clear();
            for (int i = 0; i < All.Count; i++)
            {
                var e = All[i];
                if (!e.IsRegular) continue;
                Vector2 p = e.transform.position;
                long key = Key(Mathf.FloorToInt(p.x / SeparationCell), Mathf.FloorToInt(p.y / SeparationCell));
                if (!grid.TryGetValue(key, out var cell)) grid[key] = cell = cellPool.Count > 0 ? cellPool.Pop() : new List<Enemy>();
                cell.Add(e);
            }
            float k = Mathf.Min(1f, 10f * dt);
            for (int i = 0; i < All.Count; i++)
            {
                var a = All[i];
                if (!a.IsRegular) continue;
                Vector2 pa = a.transform.position;
                int cx = Mathf.FloorToInt(pa.x / SeparationCell), cy = Mathf.FloorToInt(pa.y / SeparationCell);
                Vector2 push = Vector2.zero;
                for (int dx = -1; dx <= 1; dx++)
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (!grid.TryGetValue(Key(cx + dx, cy + dy), out var cell)) continue;
                        foreach (var b in cell)
                        {
                            if (b == a) continue;
                            Vector2 d = pa - (Vector2)b.transform.position;
                            float min = SeparationFactor * (a.radius + b.radius);
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
        private static Sprite Pixel(bool left)
        {
            ref Sprite cache = ref (left ? ref pixelLeft : ref pixelMid);
            if (cache == null)
            {
                var t = new Texture2D(1, 1, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
                t.SetPixel(0, 0, Color.white);
                t.Apply();
                cache = Sprite.Create(t, new Rect(0, 0, 1, 1), new Vector2(left ? 0f : 0.5f, 0.5f), 1f);
            }
            return cache;
        }

        /// <summary>Cienki pasek HP nad wrogiem z duza iloscia zdrowia (czarna ramka, biale wypelnienie).</summary>
        private void CreateBar()
        {
            barWidth = Mathf.Max(1.2f, radius * 2.2f);
            var root = new GameObject("HpBar");
            root.transform.SetParent(transform.parent, false);
            var bg = new GameObject("Back");
            bg.transform.SetParent(root.transform, false);
            Art.MakeRenderer(bg, Pixel(false), 12).color = Color.black;
            bg.transform.localScale = new Vector3(barWidth + 0.16f, 0.3f, 1f);
            var fill = new GameObject("Fill");
            fill.transform.SetParent(root.transform, false);
            Art.MakeRenderer(fill, Pixel(true), 13).color = Color.white;
            fill.transform.localPosition = new Vector3(-barWidth / 2f, 0f, 0f);
            barRoot = root.transform;
            barFill = fill.transform;
        }

        private void LateUpdate()
        {
            if (barRoot == null) return;
            barRoot.position = transform.position + Vector3.up * (radius + 0.5f);
            barFill.localScale = new Vector3(barWidth * Mathf.Clamp01(hp / (float)maxHp), 0.14f, 1f);
        }

        private void OnDestroy()
        {
            if (barRoot != null) Destroy(barRoot.gameObject);
        }

        public bool Blocks(Vector2 bulletVelocity)
        {
            if (type != EnemyType.Guard) return false;
            return Vector2.Dot(-bulletVelocity.normalized, ShieldVector) > 0.695f;   // 46 stopni wokol kierunku tarczy (8 kierunkow), tak jak rysowany luk
        }

        private int FacingDir8 => (Mathf.RoundToInt(Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg / 45f) + 8) % 8;
        private Vector2 ShieldVector { get { float a = FacingDir8 * 45f * Mathf.Deg2Rad; return new Vector2(Mathf.Cos(a), Mathf.Sin(a)); } }

        private void UpdateShield()
        {
            if (shieldSr == null) return;
            shieldSr.transform.rotation = Quaternion.identity;   // korpus obraca sie do ruchu, tarcza nie
            int d = FacingDir8;
            if (d != shieldDir) { shieldDir = d; shieldSr.sprite = Art.ShieldArc(d); }
        }

        /// <summary>Poborca dotknal gracza: zabiera zysk, nie znika, tylko ucieka za brzeg areny.</summary>
        private void StartFleeing(Game game)
        {
            fleeing = true;
            heldGain = game.OnCollectorTouch(out heldLevel);
            speed = 3.8f;
            if (heldGain == null) return;
            walkName = "enemy_collector_empty";   // korpus bez dolara, w srodku ikona zabranego zysku
            var gain = Catalog.Gains.Find(g => g.name == heldGain);
            var icon = gain != null ? Resources.Load<Sprite>("Icons/" + gain.icon) : null;
            if (icon == null) return;
            var go = new GameObject("HeldGain");
            go.transform.SetParent(transform, false);
            go.transform.localScale = Vector3.one * 0.75f;
            Art.MakeRenderer(go, icon, 7);
        }

        /// <summary>Oddaje graczowi zysk zabrany przez tego Poborce (zabity w ucieczce albo wymieciony z areny).</summary>
        public void ReturnHeldGain()
        {
            if (heldGain == null) return;
            string g = heldGain;
            heldGain = null;
            Game.Instance.RestoreGain(g, heldLevel);
        }

        public void Damage(int dmg, bool crit = false)
        {
            hp -= dmg;
            if (hp > 0) { Sfx.Play(Sound.Hit); flash = 0.07f; return; }
            Sfx.Play(Sound.Kill);
            if (type == EnemyType.Boss)
            {
                Game.Instance.OnBossKilled(this);
                return;
            }
            Shard.Burst(transform.parent, transform.position, 5);
            if (crit)
            {
                var st = Game.Instance.stats;
                if (st.critBlast > 0) Explosion.Spawn(transform.parent, transform.position, 1.6f, 2);
                if (st.critCoins > 0) Game.Instance.SpawnCoin(transform.position, st.critCoins);
            }
            if (type != EnemyType.Collector) 
                Game.Instance.OnEnemyKilled(transform.position);
            if (type == EnemyType.Collector)
            {
                ReturnHeldGain();
                Game.Instance.OnCollectorKilled(this);
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

        /// <summary>Sciana Toll Keepera: rzad pociskow przez cala arene z jedna przerwa wokol wallGap.</summary>
        private void FireWall()
        {
            for (float x = -Game.ArenaHalf.x; x <= Game.ArenaHalf.x; x += 0.9f)
            {
                if (Mathf.Abs(x - wallGap) < 1.9f) continue;
                Bullet.Spawn(transform.parent, new Vector2(x, Game.ArenaHalf.y + 0.3f), new Vector2(0, -4.2f), false, 0, bossBullet);
            }
            Sfx.Play(Sound.Boss);
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

            // faza 2 (<= 50% HP): dochodzi sciana z przerwa; pierscienie rzadziej, szybszy wachlarz
            if (tollPhase == 1 && hp * 2 <= maxHp)
            {
                tollPhase = 2;
                shootTimer = 1f;                 // 1 s pauzy ostrzegawczej
                wallTelegraph = 0f;
                game.Announce("THE TOLL DOUBLES");
            }
            if (wallTelegraph > 0)
            {
                wallTelegraph -= Time.deltaTime;
                if (wallTelegraph <= 0) FireWall();
                return;
            }

            shootTimer -= Time.deltaTime;
            if (shootTimer > 0) return;
            bool enraged = tollPhase == 2;
            shootTimer = enraged ? 1.4f : 2.0f;
            pattern++;

            if (enraged && pattern % 3 == 0)
            {
                // sciana: telegraf 0.8 s, przerwa blisko gracza (zawsze do uniku)
                wallGap = Mathf.Clamp(game.player.transform.position.x + Random.Range(-5f, 5f), -Game.ArenaHalf.x + 2.5f, Game.ArenaHalf.x - 2.5f);
                wallTelegraph = 0.8f;
                shootTimer = 1.8f;
                WarnLine.Spawn(transform.parent, Game.ArenaHalf.y - 0.4f, wallGap, 1.9f, 0.8f);
                return;
            }

            float baseAngle = AngleToPlayer(game, pos);
            if (enraged ? pattern % 3 == 1 : pattern % 2 == 0)
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
                    pattern++;
                    if (pattern % 2 == 0)
                    {
                        // LOAN CALL: znacznik na pozycji gracza, po 0.8 s salwa w to miejsce (+ jeden obok); zmusza do ruchu
                        shootTimer = 1.8f;
                        Vector2 target = game.player.transform.position;
                        AreaBlast.Spawn(transform.parent, target, 0.8f, 1.7f, 1);
                        AreaBlast.Spawn(transform.parent, target + Random.insideUnitCircle.normalized * 3.2f, 1.2f, 1.7f, 1);
                        game.Announce("LOAN CALL!");
                    }
                    else
                    {
                        shootTimer = 1.3f;
                        float a = AngleToPlayer(game, pos);
                        for (int i = 0; i < 5; i++) FireBoss(pos, a + (i - 2) * 11f, 6f);
                    }
                }
                else
                {
                    shootTimer = 0.55f;
                    spin += 23f;
                    pattern++;
                    if (pattern % 5 == 0)
                    {
                        // pierscien z obracajaca sie przerwa
                        gapAngle += 37f;
                        for (int i = 0; i < 22; i++)
                        {
                            float ang = i * 360f / 22f;
                            if (Mathf.Abs(Mathf.DeltaAngle(ang, gapAngle)) < 22f) continue;
                            FireBoss(pos, ang, 3.8f);
                        }
                    }
                    else for (int i = 0; i < 6; i++) FireBoss(pos, spin + i * 60f, 4.2f);
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
                if (phase > 0)
                {
                    auctionTimer = 0.5f;   // kazdy prog HP = licytacja
                    shootTimer = 1f;       // 1 s pauzy ostrzegawczej
                    lineTelegraph = 0f;
                    game.Announce(phase == 1 ? "THE BIDDING RISES" : phase == 2 ? "SOLD, SOLD, SOLD" : "FINAL OFFER");
                }
            }
            if (lineTelegraph > 0)
            {
                lineTelegraph -= Time.deltaTime;
                if (lineTelegraph <= 0) FireLine();
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
            float a = AngleToPlayer(game, pos);
            // etap 1: oferta (wachlarz 3 celowanych); 2: pierscien z licytacja; 3: linie z telegrafem; 4: dwa wzorce naraz
            if (phase == 0)
            {
                shootTimer = 1.35f;
                for (int i = 0; i < 3; i++) FireBoss(pos, a + (i - 1) * 9f, 7.5f);
            }
            else if (phase == 1)
            {
                shootTimer = 1.2f;
                if (pattern % 2 == 0)
                {
                    float offset = Random.Range(0f, 360f);
                    for (int i = 0; i < 14; i++) FireBoss(pos, offset + i * 360f / 14f, 4.2f);
                }
                else for (int i = 0; i < 4; i++) FireBoss(pos, a + (i - 1.5f) * 9f, 7.5f);
            }
            else if (phase == 2)
            {
                shootTimer = 2.2f;
                lineVertical = pattern % 2 == 0;
                lineSide = Random.value < 0.5f ? -1 : 1;
                Vector2 pp = game.player.transform.position;
                if (lineVertical)
                {
                    lineGap = Mathf.Clamp(pp.y + Random.Range(-2.5f, 2.5f), -Game.ArenaHalf.y + 1.8f, Game.ArenaHalf.y - 1.8f);
                    WarnLine.SpawnVertical(transform.parent, lineSide * (Game.ArenaHalf.x - 0.4f), lineGap, 1.7f, 0.8f);
                }
                else
                {
                    lineGap = Mathf.Clamp(pp.x + Random.Range(-5f, 5f), -Game.ArenaHalf.x + 2.5f, Game.ArenaHalf.x - 2.5f);
                    WarnLine.Spawn(transform.parent, Game.ArenaHalf.y - 0.4f, lineGap, 1.9f, 0.8f);
                }
                lineTelegraph = 0.8f;
            }
            else
            {
                shootTimer = 1.4f;
                float offset = Random.Range(0f, 360f);
                for (int i = 0; i < 12; i++) FireBoss(pos, offset + i * 360f / 12f, 4.2f);
                for (int i = 0; i < 3; i++) FireBoss(pos, a + (i - 1) * 9f, 7.5f);
            }
        }

        /// <summary>Auctioneer, etap 3: linia pociskow przez arene (pozioma z gory albo pionowa z boku) z jedna przerwa.</summary>
        private void FireLine()
        {
            if (lineVertical)
            {
                float x = lineSide * (Game.ArenaHalf.x + 0.3f);
                for (float y = -Game.ArenaHalf.y; y <= Game.ArenaHalf.y; y += 0.9f)
                    if (Mathf.Abs(y - lineGap) >= 1.7f) Bullet.Spawn(transform.parent, new Vector2(x, y), new Vector2(-lineSide * 4.2f, 0), false, 0, bossBullet);
            }
            else
            {
                for (float x = -Game.ArenaHalf.x; x <= Game.ArenaHalf.x; x += 0.9f)
                    if (Mathf.Abs(x - lineGap) >= 1.9f) Bullet.Spawn(transform.parent, new Vector2(x, Game.ArenaHalf.y + 0.3f), new Vector2(0, -4.2f), false, 0, bossBullet);
            }
            Sfx.Play(Sound.Boss);
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
                        moveSpeed = Mathf.Lerp(0.1f, speed, Mathf.InverseLerp(1, 5, dist));
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
                    move = dist < 7f ? -dir
                        : dist > 10f ? dir 
                        : Vector2.zero;
                    shootTimer -= dt;
                    if (shootTimer <= 0 && attacking && spawned < 12)
                    {
                        shootTimer = 4f;
                        spawned++;
                        Spawn(transform.parent, EnemyType.Chaser, pos, game.Difficulty, 0);
                    }
                    break;

                case EnemyType.Collector:
                    if (!fleeing) move = dir;
                    else
                    {
                        // ucieczka od gracza, lekko w strone najblizszego brzegu, zeby nie utknal na srodku
                        Vector2 outward = pos.sqrMagnitude > 0.01f ? pos.normalized : Vector2.up;
                        move = (-dir + 0.35f * outward).normalized;
                    }
                    break;

                case EnemyType.Guard:
                    move = dir;
                    if (dir != Vector2.zero) facing = ((Vector2)Vector3.RotateTowards(facing, dir, 0.7f * dt, 0f)).normalized;   // tarcza obraca sie powoli: da sie go okrazyc
                    break;

                default:
                    move = dir;
                    break;
            }

            clock += dt;
            if (walkName != null && sr != null)
                sr.sprite = Art.Square(walkName + "_walk_" + (Mathf.FloorToInt(clock / 0.25f) % 2), spriteSize);

            pos += move * (moveSpeed * dt);
            if (!fleeing)
            {
                pos.x = Mathf.Clamp(pos.x, -Game.ArenaHalf.x, Game.ArenaHalf.x);
                pos.y = Mathf.Clamp(pos.y, -Game.ArenaHalf.y, Game.ArenaHalf.y);
            }
            pos = Obstacles.Push(pos, radius * 0.8f);
            transform.position = pos;
            if (fleeing && (Mathf.Abs(pos.x) > Game.ArenaHalf.x + 1f || Mathf.Abs(pos.y) > Game.ArenaHalf.y + 1f))
            {
                game.OnCollectorEscaped(heldGain);   // uciekl: zysk przepada
                heldGain = null;
                Destroy(gameObject);
                return;
            }
            if (rotates && dir != Vector2.zero && dashLeft <= 0) transform.rotation = Art.FaceRotation(dir);
            UpdateShield();

            float r = radius + Player.Radius;
            if (dist < r * 0.85f && type != EnemyType.Collector)   // miekkie wypychanie: wrog nie zostaje "w srodku" gracza
                transform.position = (Vector2)transform.position - (dir == Vector2.zero ? Vector2.right : dir) * ((r * 0.85f - dist) * Mathf.Min(1f, 10f * dt));
            if (dist < r)
            {
                if (type == EnemyType.Collector)
                {
                    if (!fleeing) StartFleeing(game);
                    return;
                }
                game.player.Hurt(contactDamage);
            }
        }
    }
}
