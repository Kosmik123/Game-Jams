using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PayWithPixels
{
    /// <summary>Caly stan runu: strefy, fale, monety, dlug, sklep, smierc. Wszystko budowane w kodzie (patrz Bootstrap).</summary>
    public class Game : MonoBehaviour
    {
        public enum State { Title, ZoneIntro, Playing, Clearing, Shop, Dead, Won }

        public const float OrthoSize = 5.625f; // 180 px / 16 PPU / 2
        public static readonly Vector2 ArenaHalf = new Vector2(9.5f, 5.2f);

        public const int Zones = 3;
        public const int WavesPerZone = 5;           // + boss strefy
        public const int MaxDebt = 30;
        public const float InterestRate = 0.2f;

        private static readonly string[] ZoneNames = { "THE TOLL ROAD", "THE BANK", "THE AUCTION HOUSE" };

        public static Game Instance { get; private set; }

        public State state;
        public int zone;
        public int waveInZone;   // 1..WavesPerZone, WavesPerZone+1 = boss
        public int wave;         // numer fali w calym runie
        public int coins;
        public int debt;
        public RunStats stats;
        public Player player;
        public TwoColorDisplay display;
        private Camera cam;
        private GameUI ui;
        private Transform world;

        private int spawnLeft;
        private float spawnTimer;
        private float clearTimer;
        private float introTimer;
        private float flickerClock;
        private bool lastWaveWasBoss;
        private float invertClock;
        private float freezeTimer;
        private bool freezeReady;
        private int kills;
        private float lingerTimer;
        private readonly Drone[] drones = new Drone[3];
        public bool Frozen => freezeTimer > 0;
        public bool Paused { get; private set; }
        private float hitStopUntil;
        private float shake;

        // ---- narzedzia testowe: Edytor, Development Build albo WebGL z ?debug=1 ----
        public bool GodMode { get; private set; }
        public static bool DebugEnabled => Application.isEditor || Debug.isDebugBuild || Application.absoluteURL.Contains("debug=1");
        public bool ControlsInverted { get; private set; }

        public bool IsBossWave => waveInZone > WavesPerZone;
        /// <summary>Trudnosc do skalowania wrogow (rosnie przez caly run).</summary>
        public int Difficulty => (zone - 1) * (WavesPerZone + 1) + waveInZone;

        public static void Create()
        {
            new GameObject("Game").AddComponent<Game>();
        }

        private void Awake()
        {
            Instance = this;
            Application.targetFrameRate = 60;

            foreach (var c in FindObjectsByType<Camera>(FindObjectsSortMode.None))
                c.gameObject.SetActive(false);

            var camGo = new GameObject("World Camera");
            camGo.transform.SetParent(transform, false);
            camGo.transform.position = new Vector3(0, 0, -10);
            cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = OrthoSize;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            cam.allowMSAA = false;
            cam.allowHDR = false;
            camGo.AddComponent<AudioListener>();

            display = TwoColorDisplay.Create(transform, cam);
            Sfx.Init(transform);
            Settings.Apply();
            world = new GameObject("World").transform;
            world.SetParent(transform, false);
            BuildArena();
            ui = GameUI.Create(transform);

            state = State.Title;
            ui.ShowTitle(StartRun);
        }

        private void BuildArena()
        {
            var bg = new GameObject("Arena BG");
            bg.transform.SetParent(world, false);
            Art.MakeRenderer(bg, Art.Square("arena_bg", 16), -10);
            var wall = Art.Square("tile_wall", 16);
            // ramka areny z kafli scian (jesli sa sprity od grafika) lub cienkich bialych pasow
            float w = ArenaHalf.x + 0.5f, h = ArenaHalf.y + 0.5f;
            AddBar(wall, new Vector2(0, h), new Vector2(w * 2 + 1, 0.2f));
            AddBar(wall, new Vector2(0, -h), new Vector2(w * 2 + 1, 0.2f));
            AddBar(wall, new Vector2(w, 0), new Vector2(0.2f, h * 2));
            AddBar(wall, new Vector2(-w, 0), new Vector2(0.2f, h * 2));
        }

        private void AddBar(Sprite sprite, Vector2 pos, Vector2 size)
        {
            var go = new GameObject("Wall");
            go.transform.SetParent(world, false);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(size.x, size.y, 1);
            Art.MakeRenderer(go, sprite, 0);
        }

        // ---------- run ----------

        private void StartRun()
        {
            foreach (var e in Enemy.All.ToArray()) if (e != null) Destroy(e.gameObject);
            foreach (var b in Bullet.All.ToArray()) if (b != null) Destroy(b.gameObject);
            foreach (var a in AreaBlast.All.ToArray()) if (a != null) Destroy(a.gameObject);
            foreach (var c in Coin.All.ToArray()) if (c != null) Destroy(c.gameObject);
            foreach (var m in Mine.All.ToArray()) if (m != null) Destroy(m.gameObject);
            foreach (var x in Explosion.All.ToArray()) if (x != null) Destroy(x.gameObject);
            for (int i = 0; i < drones.Length; i++) { if (drones[i] != null) Destroy(drones[i].gameObject); drones[i] = null; }
            if (player != null) Destroy(player.gameObject);

            Palette.Reset();
            flickerClock = 0;
            stats = new RunStats();
            zone = 1;
            waveInZone = 0;
            wave = 0;
            coins = 0;
            debt = 0;
            lastWaveWasBoss = false;
            kills = 0;
            freezeTimer = 0;
            invertClock = 0;
            ControlsInverted = false;
            ApplyPrices();
            player = Player.Create(world);
            ui.HideTitle();
            Sfx.StartMusic();
            ui.HideDeath();
            ui.HideShop();
            BeginZone();
        }

        public void ApplyPrices()
        {
            display.SetPixelLevel(stats.pixelStacks);
            display.SetFog(stats.FogRadius);
        }

        private void BeginZone()
        {
            state = State.ZoneIntro;
            introTimer = 2.6f;
            waveInZone = 0;
            Palette.SetZone(zone, 1);
            Obstacles.Build(world, zone);
            ui.ShowZone(zone, ZoneNames[zone - 1]);
        }

        private void StartWave()
        {
            wave++;
            waveInZone++;
            ui.HideZone();
            Palette.SetZone(zone, waveInZone);
            state = State.Playing;
            lastWaveWasBoss = IsBossWave;
            spawnLeft = IsBossWave ? 6 : 5 + 2 * waveInZone + 4 * (zone - 1);
            spawnTimer = 0.5f;
            player.RefillShield();
            freezeReady = stats.freeze > 0;

            if (IsBossWave)
            {
                Enemy.SpawnBoss(world, new Vector2(0, ArenaHalf.y - 1.5f), zone);
                Sfx.Play(Sound.Boss);
            }
            else if (debt > 0 && waveInZone == WavesPerZone)
            {
                Enemy.SpawnCollector(world, PickSpawnPoint());
                ui.Toast("THE COLLECTOR COMES FOR YOUR DEBT");
            }
        }

        public void AddCoins(int n)
        {
            coins += n;
            Sfx.Play(Sound.Coin);
        }

        public void OnBossKilled()
        {
            foreach (var e in Enemy.All.ToArray()) if (e != null) Destroy(e.gameObject);
            foreach (var b in Bullet.All.ToArray()) if (b != null) Destroy(b.gameObject);
            foreach (var a in AreaBlast.All.ToArray()) if (a != null) Destroy(a.gameObject);
            spawnLeft = 0;
            HitStop(0.2f);
            Shake(0.6f);
            if (zone >= Zones)
            {
                state = State.Won;
                Sfx.Play(Sound.Win);
                ui.ShowDeath(wave, StartRun, true);
            }
            else
            {
                SpawnCoin(Vector2.zero, 8);
                ui.Toast("ZONE CLEARED");
            }
        }

        /// <summary>Poborca zabity: dlug splacony.</summary>
        public void OnCollectorKilled()
        {
            debt = 0;
            ui.Toast("DEBT PAID");
        }

        /// <summary>Poborca dotknal gracza: zabiera losowy zysk, zostawia cene.</summary>
        public void OnCollectorTouch()
        {
            string taken = stats.StealRandomGain();
            RefreshDrones();
            if (player.hp > stats.maxHp) player.hp = stats.maxHp;
            ui.Toast(taken != null ? "THE COLLECTOR TOOK: " + taken : "THE COLLECTOR FOUND NOTHING TO TAKE");
        }

        public void OnEnemyKilled(Vector2 pos)
        {
            kills++;
            HitStop(0.03f);
            Shake(0.05f);
            if (stats.vampire > 0 && kills % stats.VampireKills == 0 && player.hp < stats.maxHp)
                player.Heal(1);
            if (stats.explosion > 0)
                Explosion.Spawn(world, pos, stats.ExplosionRadius, stats.ExplosionDamage);
        }

        /// <summary>Dolacza/odlacza dronow po zmianie zyskow.</summary>
        public void RefreshDrones()
        {
            for (int i = 0; i < drones.Length; i++)
                if (i < stats.drones && drones[i] == null)
                    drones[i] = Drone.Create(world, i);
        }

        public void SpawnCoin(Vector2 pos, int count)
        {
            for (int i = 0; i < count; i++)
                Coin.Spawn(world, pos + Random.insideUnitCircle * 0.4f);
        }

        public void OnPlayerDied()
        {
            state = State.Dead;
            Sfx.Play(Sound.Death);
            Shake(0.6f);
            ui.ShowDeath(wave, StartRun);
        }

        /// <summary>Krotkie zatrzymanie czasu przy trafieniu/zabiciu.</summary>
        public void HitStop(float seconds) => hitStopUntil = Mathf.Max(hitStopUntil, Time.unscaledTime + seconds);

        public void Shake(float amount) => shake = Mathf.Max(shake, amount);

        private void LateUpdate()
        {
            if (cam == null) return;
            Vector3 p = new Vector3(0, 0, -10);
            if (Settings.Shake && shake > 0.01f && !Paused)
            {
                // przesuniecie przyciete do 1/16 j. = 1 piksel RT, zeby obraz nie mazal
                p.x = Mathf.Round(Random.Range(-shake, shake) * 16f) / 16f;
                p.y = Mathf.Round(Random.Range(-shake, shake) * 16f) / 16f;
            }
            shake = Mathf.MoveTowards(shake, 0f, 1.6f * Time.unscaledDeltaTime);
            cam.transform.position = p;
        }

        private bool CanPause => state == State.ZoneIntro || state == State.Playing || state == State.Clearing || state == State.Shop;

        private void TogglePause()
        {
            Paused = !Paused;
            Time.timeScale = Paused ? 0f : 1f;
            if (Paused) ShowPauseMenu();
            else ui.HidePause();
        }

        private void ShowPauseMenu()
        {
            ui.ShowPause(TogglePause,
                () => { TogglePause(); StartRun(); },
                () => { Settings.Volume = Settings.Volume >= 0.99f ? 0f : Settings.Volume + 0.1f; ShowPauseMenu(); },
                () => { Settings.ReduceFlicker = !Settings.ReduceFlicker; ShowPauseMenu(); },
                () => { Settings.Shake = !Settings.Shake; ShowPauseMenu(); });
        }

        private void Update()
        {
            if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P)) && (CanPause || Paused)) TogglePause();
            if (Paused) return;
            Time.timeScale = Time.unscaledTime < hitStopUntil ? 0.05f : 1f;

            if (cam != null) cam.orthographicSize = Mathf.MoveTowards(cam.orthographicSize, OrthoSize * (stats != null ? stats.CameraZoom : 1f), 4f * Time.deltaTime);
            if (player != null) display.SetFogCenter(player.transform.position);
            if (player != null && state != State.Title)
                ui.SetPriceHud(PaidPrices());
            if (player != null && state != State.Title)
                ui.SetHud(player.hp, stats.maxHp, ZoneLabel(), coins, debt, FreezeHint());

            if (Input.GetKeyDown(KeyCode.M)) Sfx.ToggleMute();
            if (state == State.Title)
            {
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) StartRun();
                return;
            }
            if (DebugEnabled) UpdateDebugKeys();
            UpdateFreeze();
            UpdateFlicker();
            UpdateInvertedControls();

            switch (state)
            {
                case State.ZoneIntro:
                    introTimer -= Time.deltaTime;
                    if (introTimer <= 0) StartWave();
                    break;
                case State.Playing:
                    UpdateSpawner();
                    UpdateLingerFailsafe();
                    if (spawnLeft <= 0 && Enemy.All.Count == 0)
                    {
                        state = State.Clearing;
                        clearTimer = 1.2f;
                        if (debt > 0)
                        {
                            debt = Mathf.CeilToInt(debt * (1f + InterestRate));
                            ui.Toast("INTEREST: DEBT IS NOW " + debt);
                        }
                    }
                    break;
                case State.Clearing:
                    clearTimer -= Time.deltaTime;
                    if (clearTimer <= 0) OpenShop();
                    break;
                case State.Shop:
                    UpdateShopKeys();
                    break;
                case State.Won:
                case State.Dead:
                    if (Input.GetKeyDown(KeyCode.R)) StartRun();
                    break;
            }
        }

        private List<KeyValuePair<string, int>> PaidPrices()
        {
            var list = new List<KeyValuePair<string, int>>();
            foreach (var price in Catalog.Prices)
            {
                int n = stats.PriceCount(price.name);
                if (n > 0 && !string.IsNullOrEmpty(price.icon)) list.Add(new KeyValuePair<string, int>(price.icon, n));
            }
            return list;
        }

        private string FreezeHint() => (stats.freeze > 0 ? (freezeReady ? "[F] FREEZE READY" : "FREEZE USED") : "") + (DebugEnabled ? (GodMode ? " [DEBUG GOD]" : " [DEBUG]") : "");

        private string ZoneLabel() => IsBossWave ? "ZONE " + zone + " BOSS" : "ZONE " + zone + "-" + Mathf.Max(1, waveInZone);

        /// <summary>Skroty klawiszowe sklepu (odporne na zgubione klikniecia): 1-3 zakup, R reroll, H haggle, D splata, Enter dalej.</summary>
        private void UpdateShopKeys()
        {
            if (shopOffers == null) return;
            KeyCode[] keys = { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3 };
            for (int i = 0; i < keys.Length && i < shopOffers.Count; i++)
                if (Input.GetKeyDown(keys[i])) { Buy(shopOffers[i]); return; }
            if (Input.GetKeyDown(KeyCode.R)) Reroll();
            else if (Input.GetKeyDown(KeyCode.H)) Haggle();
            else if (Input.GetKeyDown(KeyCode.D)) PayDebt();
            else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) CloseShop();
        }

        private void KillAllEnemies()
        {
            foreach (var e in Enemy.All.ToArray()) if (e != null) e.Damage(99999);
        }

        /// <summary>F1 god mode, F2 +50 monet, F3 nastepna fala, F4 nastepna strefa, F5 sklep, F6 zabij wrogow.</summary>
        private void UpdateDebugKeys()
        {
            if (Input.GetKeyDown(KeyCode.F1)) { GodMode = !GodMode; ui.Toast("DEBUG: GOD MODE " + (GodMode ? "ON" : "OFF")); }
            if (Input.GetKeyDown(KeyCode.F2)) { coins += 50; ui.Toast("DEBUG: +50 COINS"); }
            if (Input.GetKeyDown(KeyCode.F6) && state == State.Playing) KillAllEnemies();
            if (Input.GetKeyDown(KeyCode.F3) && state == State.Playing)
            {
                spawnLeft = 0;
                KillAllEnemies();
            }
            if (Input.GetKeyDown(KeyCode.F5) && (state == State.Playing || state == State.Clearing))
            {
                spawnLeft = 0;
                foreach (var e in Enemy.All.ToArray()) if (e != null) Destroy(e.gameObject);
                OpenShop();
            }
            if (Input.GetKeyDown(KeyCode.F4) && (state == State.Playing || state == State.Clearing || state == State.Shop))
            {
                spawnLeft = 0;
                foreach (var e in Enemy.All.ToArray()) if (e != null) Destroy(e.gameObject);
                foreach (var b in Bullet.All.ToArray()) if (b != null) Destroy(b.gameObject);
                ui.HideShop();
                if (zone < Zones) { zone++; lastWaveWasBoss = false; BeginZone(); }
                else ui.Toast("DEBUG: ALREADY IN THE LAST ZONE");
            }
        }

        private void UpdateFreeze()
        {
            if (freezeTimer > 0) freezeTimer -= Time.deltaTime;
            if (state == State.Playing && freezeReady && stats.freeze > 0 && Input.GetKeyDown(KeyCode.F))
            {
                freezeReady = false;
                freezeTimer = stats.FreezeDuration;
                Sfx.Play(Sound.Boss);
                ui.Toast("TIME FROZEN");
            }
        }

        private void UpdateInvertedControls()
        {
            int stacks = stats.invertStacks;
            bool active = stacks > 0 && state == State.Playing;
            if (!active) { ControlsInverted = false; invertClock = 0; return; }
            float period = 11f, dur = 1.5f + 0.5f * stacks;
            float prev = invertClock;
            invertClock = (invertClock + Time.deltaTime) % (period + dur);
            ControlsInverted = invertClock > period;
            if (ControlsInverted && prev <= period) ui.Toast("CONTROLS INVERTED!");
        }

        private void UpdateFlicker()
        {
            float dur = stats.FlickerDuration;
            bool active = dur > 0 && (state == State.Playing || state == State.Clearing);
            const float period = 8f;
            bool flick = false;
            if (!active || Settings.ReduceFlicker) flickerClock = 0;
            else
            {
                flickerClock = (flickerClock + Time.deltaTime) % (period + dur);
                flick = flickerClock > period;
            }
            Palette.Inverted = flick != (Frozen && !Settings.ReduceFlicker);   // zamrozenie czasu tez odwraca palete
        }

        /// <summary>Zabezpieczenie: jesli po koncu spawnu wrogowie (poza bossem i Poborca) zaciely sie dlugo na planszy, odchodza.</summary>
        private void UpdateLingerFailsafe()
        {
            bool lingering = spawnLeft <= 0 && Enemy.All.Count > 0 && !Frozen;
            if (!lingering) { lingerTimer = 0; return; }
            lingerTimer += Time.deltaTime;
            if (lingerTimer < 75f) return;
            lingerTimer = 0;
            bool any = false;
            foreach (var e in Enemy.All.ToArray())
                if (e != null && e.IsRegular) { Destroy(e.gameObject); any = true; }
            if (any) ui.Toast("THE REST GAVE UP THE CHASE");
        }

        private void UpdateSpawner()
        {
            if (spawnLeft <= 0) return;
            spawnTimer -= Time.deltaTime;
            if (spawnTimer > 0) return;
            spawnTimer = Mathf.Max(0.35f, 0.8f - Difficulty * 0.03f);
            spawnLeft--;

            if (IsBossWave)
            {
                Enemy.Spawn(world, EnemyType.Chaser, PickSpawnPoint(), Difficulty);
                spawnTimer = 4f;
                return;
            }
            var type = PickEnemyType();
            var at = PickSpawnPoint();
            if (type == EnemyType.Swarm)
            {
                // roj: 5 malych naraz, liczy sie jako 2 miejsca w puli fali
                for (int i = 0; i < 5; i++)
                    Enemy.Spawn(world, type, at + Random.insideUnitCircle * 0.7f, Difficulty);
                spawnLeft = Mathf.Max(0, spawnLeft - 1);
                return;
            }
            Enemy.Spawn(world, type, at, Difficulty);
        }

        /// <summary>Pula wrogow zalezy od strefy i fali.</summary>
        private EnemyType PickEnemyType()
        {
            // (typ, waga) - dostepnosc zalezy od strefy i numeru fali w strefie
            var pool = new List<KeyValuePair<EnemyType, float>>();
            void Add(EnemyType t, float w, int minWave = 1) { if (waveInZone >= minWave) pool.Add(new KeyValuePair<EnemyType, float>(t, w)); }

            Add(EnemyType.Chaser, zone == 1 ? 5f : 3f);
            Add(EnemyType.Shooter, 3f, zone == 1 ? 2 : 1);
            Add(EnemyType.Tank, 1.2f, zone == 1 ? 4 : 3);
            if (zone >= 2)
            {
                Add(EnemyType.Dasher, 2.5f);
                Add(EnemyType.Swarm, 2f, 2);
                Add(EnemyType.Guard, 1.8f, 3);
            }
            if (zone >= 3)
            {
                Add(EnemyType.Siege, 2f);
                Add(EnemyType.Spawner, 0.8f, 3);
            }

            float total = 0;
            foreach (var kv in pool) total += kv.Value;
            float r = Random.value * total;
            foreach (var kv in pool)
            {
                r -= kv.Value;
                if (r <= 0) return kv.Key;
            }
            return EnemyType.Chaser;
        }

        private Vector2 PickSpawnPoint()
        {
            Vector2 p = Vector2.zero;
            for (int i = 0; i < 12; i++)
            {
                bool horizontal = Random.value < 0.5f;
                p = horizontal
                    ? new Vector2(Random.Range(-ArenaHalf.x, ArenaHalf.x), Random.value < 0.5f ? -ArenaHalf.y : ArenaHalf.y)
                    : new Vector2(Random.value < 0.5f ? -ArenaHalf.x : ArenaHalf.x, Random.Range(-ArenaHalf.y, ArenaHalf.y));
                if (player == null || (p - (Vector2)player.transform.position).sqrMagnitude > 16f) break;
            }
            return p;
        }

        // ---------- sklep ----------

        private void OpenShop()
        {
            // reszta monet wpada automatycznie
            foreach (var c in Coin.All.ToArray()) if (c != null) { coins++; Destroy(c.gameObject); }
            foreach (var b in Bullet.All.ToArray()) if (b != null) Destroy(b.gameObject);
            foreach (var a in AreaBlast.All.ToArray()) if (a != null) Destroy(a.gameObject);

            state = State.Shop;
            shopOffers = Catalog.Roll(3, wave, stats);
            ui.AnimateNextShop = true;
            rerolls = 0;
            purchases = 0;
            haggled = false;
            RefreshShopUi();
        }

        private List<Offer> shopOffers;
        private int rerolls;
        private int purchases;   // zakupy w tej wizycie: kazdy kolejny +25%
        private bool haggled;

        private int RerollCost => 3 + 2 * rerolls;

        /// <summary>Aktualny koszt oferty: +25% za kazdy wczesniejszy zakup w tej wizycie.</summary>
        public int CostOf(Offer o) => Mathf.CeilToInt(o.cost * (1f + 0.25f * purchases));

        private void RefreshShopUi()
        {
            ui.ShowShop(shopOffers, coins, debt, Buy, CloseShop, RerollCost, Reroll, !haggled, Haggle, PayDebt);
        }

        /// <summary>Splaca dlug dostepnymi monetami.</summary>
        private void PayDebt()
        {
            int amount = Mathf.Min(coins, debt);
            if (amount <= 0) return;
            coins -= amount;
            debt -= amount;
            Sfx.Play(Sound.Buy);
            RefreshShopUi();
        }

        private void Reroll()
        {
            if (coins < RerollCost) return;
            coins -= RerollCost;
            rerolls++;
            shopOffers = Catalog.Roll(3, wave, stats);
            Sfx.Play(Sound.Coin);
            RefreshShopUi();
        }

        /// <summary>Targowanie: za 1 monete losuje nowe ceny dla ofert (moga byc gorsze).</summary>
        private void Haggle()
        {
            if (haggled || coins < 1) return;
            coins -= 1;
            haggled = true;
            foreach (var o in shopOffers)
                if (!o.sold) o.price = Catalog.Prices[Random.Range(0, Catalog.Prices.Count)];
            Sfx.Play(Sound.Hurt);
            RefreshShopUi();
        }

        /// <summary>Czy oferta jest do kupienia (za monety albo na kredyt).</summary>
        public bool CanAfford(Offer o)
        {
            if (o.sold) return false;
            if (o.sell) return true;
            int c = CostOf(o);
            return coins >= c || debt + (c - coins) <= MaxDebt;
        }

        private void Buy(Offer o)
        {
            if (!CanAfford(o)) return;
            if (o.sell) coins += o.reward;
            else
            {
                int c = CostOf(o);
                if (coins >= c) coins -= c;
                else
                {
                    debt += c - coins;   // brak monet: reszta na kredyt
                    coins = 0;
                }
                purchases++;
                var g = o.gain;
                if (!g.consumable) stats.levels[g.name] = o.level;
                stats.Recompute();
                g.onBuy?.Invoke(stats, player);
                RefreshDrones();
                if (player.hp > stats.maxHp) player.hp = stats.maxHp;
            }
            Sfx.Play(Sound.Buy);
            o.price.apply(stats);
            stats.AddPrice(o.price.name);
            stats.Recompute();
            if (player.hp > stats.maxHp) player.hp = stats.maxHp;
            ApplyPrices();
            o.sold = true;
            RefreshShopUi();   // sklep zostaje otwarty; NEXT WAVE zamyka
        }

        private void CloseShop()
        {
            ui.HideShop();
            if (lastWaveWasBoss)
            {
                zone++;
                BeginZone();
            }
            else StartWave();
        }
    }

    public static class Bootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            if (SceneManager.GetActiveScene().name != "Game") return;
            Game.Create();
        }
    }
}
