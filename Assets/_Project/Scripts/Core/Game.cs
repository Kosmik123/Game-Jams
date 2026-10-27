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
        private float runTime;
        private int coinsEarned;
        private bool hitThisWave;
        private int noHitWaves;
        private bool coinHintShown, debtHintShown;
        private readonly List<string> newAchievements = new List<string>();
        private readonly Drone[] drones = new Drone[3];
        public bool Frozen => freezeTimer > 0;
        public bool Paused { get; private set; }
        /// <summary>Tylko dla testow automatycznych (przyspieszenie symulacji).</summary>
        public static float DebugTimeScale = 1f;

        // ---- licytacja Aukcjonera: zabrane zyski wracaja po czasie albo po jego smierci ----
        private readonly System.Collections.Generic.List<KeyValuePair<string, int>> auctioned = new System.Collections.Generic.List<KeyValuePair<string, int>>();
        private float auctionReturnTimer;

        public void Announce(string msg) => ui.Toast(msg);

        public void NotifyHit() => hitThisWave = true;

        private void Grant(string id)
        {
            if (!Meta.Unlock(id)) return;
            newAchievements.Add(id);
            var a = Meta.Find(id);
            ui.Toast("ACHIEVEMENT: " + a.title + " - " + a.description, 4f);
            Sfx.Play(Sound.Win);
        }

        private void CheckAchievements()
        {
            if (coins >= 100) Grant("rich");
            if (stats.fogStacks >= 3) Grant("fog3");
            if (zone >= 3) Grant("survivor");
        }

        private string RunSummary(bool won)
        {
            int t = Mathf.RoundToInt(runTime);
            string text = (won ? "ALL THREE ZONES CLEARED\n" : "YOU REACHED ZONE " + zone + ", WAVE " + wave + "\n") +
                "KILLS " + kills + "    COINS EARNED " + coinsEarned + "    TIME " + (t / 60) + ":" + (t % 60).ToString("00") +
                "\nBEST WAVE " + Meta.BestWave + "    WINS " + Meta.Wins + "    DEBT LEFT " + debt;
            if (newAchievements.Count > 0)
            {
                text += "\n\nNEW ACHIEVEMENTS:";
                foreach (var id in newAchievements) text += "\n" + Meta.Find(id).title;
            }
            return text;
        }

        private void EndRun(bool won)
        {
            int score = Mathf.RoundToInt((wave * 100 + kills * 2 + coinsEarned + (won ? 1000 : 0)) * ScoreMul);
            Meta.RecordRun(wave, won, Ascension, score);
            ui.ShowDeath(won, RunSummary(won) + "\nSCORE " + score + (Ascension > 0 ? "  (ASCENSION " + Ascension + " x" + ScoreMul.ToString("0.00") + ")" : ""), StartRun, GoToTitle);
        }

        public void AuctionOffGain(float seconds)
        {
            var owned = new System.Collections.Generic.List<string>();
            foreach (var kv in stats.levels) if (kv.Value > 0) owned.Add(kv.Key);
            if (owned.Count == 0) { ui.Toast("THE AUCTIONEER FINDS NOTHING TO SELL"); return; }
            string name = owned[Random.Range(0, owned.Count)];
            auctioned.Add(new KeyValuePair<string, int>(name, stats.levels[name]));
            stats.levels.Remove(name);
            stats.Recompute();
            RefreshDrones();
            auctionReturnTimer = seconds;
            ui.Toast("SOLD! YOUR " + name + " GOES TO THE HIGHEST BIDDER");
            Sfx.Play(Sound.Hurt);
        }

        private void RestoreAuctioned()
        {
            if (auctioned.Count == 0) return;
            foreach (var kv in auctioned) stats.levels[kv.Key] = kv.Value;
            auctioned.Clear();
            stats.Recompute();
            RefreshDrones();
            ui.Toast("YOUR GAINS CAME BACK");
        }
        private float hitStopUntil;
        private float shake;

        // ---- narzedzia testowe: Edytor, Development Build albo WebGL z ?debug=1 ----
        public bool GodMode { get; private set; }
        public static bool DebugEnabled => Application.isEditor || Debug.isDebugBuild || Application.absoluteURL.Contains("debug=1");
        public bool ControlsInverted { get; private set; }

        // ---- Ascension (po pierwszej wygranej): trudniejsze runy, mnoznik wyniku ----
        public int Ascension { get; private set; }
        public float InterestRate => Ascension >= 4 ? 0.3f : 0.2f;
        public float EnemyHpMul => Ascension >= 2 ? 1.25f : 1f;
        public float EnemySpeedMul => Ascension >= 4 ? 1.15f : 1f;
        public float BossHpMul => Ascension >= 5 ? 1.2f : 1f;
        public float ScoreMul => 1f + 0.25f * Ascension;

        private void ApplyStartPrice(string name, int n)
        {
            var price = Catalog.Prices.Find(pr => pr.name == name);
            if (price == null) return;
            for (int i = 0; i < n; i++)
            {
                price.apply(stats);
                stats.AddPrice(name);
            }
        }

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

            GoToTitle();
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

        private void GoToTitle()
        {
            CleanupRun();
            Palette.Reset();
            state = State.Title;
            ui.HideDeath();
            ui.HideShop();
            ui.HideZone();
            GoToTitleRefresh();
        }

        private void GoToTitleRefresh() => ui.ShowTitle(StartRun, w => { Meta.Selected = w; GoToTitleRefresh(); },
            () => { Meta.Ascension = (Meta.Ascension + 1) % (Meta.AscensionUnlocked + 1); GoToTitleRefresh(); });

        private void CleanupRun()
        {
            foreach (var e in Enemy.All.ToArray()) if (e != null) Destroy(e.gameObject);
            foreach (var b in Bullet.All.ToArray()) if (b != null) Destroy(b.gameObject);
            foreach (var a in AreaBlast.All.ToArray()) if (a != null) Destroy(a.gameObject);
            foreach (var c in Coin.All.ToArray()) if (c != null) Destroy(c.gameObject);
            foreach (var m in Mine.All.ToArray()) if (m != null) Destroy(m.gameObject);
            foreach (var x in Explosion.All.ToArray()) if (x != null) Destroy(x.gameObject);
            for (int i = 0; i < drones.Length; i++) { if (drones[i] != null) Destroy(drones[i].gameObject); drones[i] = null; }
            if (player != null) Destroy(player.gameObject);
            player = null;
        }

        private void StartRun()
        {
            CleanupRun();
            Palette.Reset();
            flickerClock = 0;
            auctioned.Clear();
            runTime = 0;
            kills = 0;
            coinsEarned = 0;
            noHitWaves = 0;
            hitThisWave = false;
            newAchievements.Clear();
            Ascension = Meta.Ascension;
            stats = new RunStats(Meta.Selected);
            if (Ascension >= 1) ApplyStartPrice("FOG", 1);
            if (Ascension >= 3) { ApplyStartPrice("HEAVY LEGS", 1); ApplyStartPrice("PIXELATION", 1); }
            if (Ascension >= 5) { ApplyStartPrice("FLICKER", 1); ApplyStartPrice("ZOOM IN", 1); }
            stats.Recompute();
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
            Sfx.SetMusic(zone);
            Obstacles.Build(world, zone);
            ui.ShowZone(zone, ZoneNames[zone - 1]);
        }

        private void StartWave()
        {
            wave++;
            waveInZone++;
            hitThisWave = false;
            if (!Meta.TutorialDone && wave == 1) ui.Toast("WASD: MOVE    MOUSE: AIM    HOLD LEFT BUTTON: SHOOT", 7f);
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
            coinsEarned += n;
            Sfx.Play(Sound.Coin);
            if (!Meta.TutorialDone && !coinHintShown)
            {
                coinHintShown = true;
                ui.Toast("COINS BUY GAINS IN THE SHOP AFTER EACH WAVE", 5f);
            }
        }

        public void OnBossKilled()
        {
            foreach (var e in Enemy.All.ToArray()) if (e != null) Destroy(e.gameObject);
            foreach (var b in Bullet.All.ToArray()) if (b != null) Destroy(b.gameObject);
            foreach (var a in AreaBlast.All.ToArray()) if (a != null) Destroy(a.gameObject);
            spawnLeft = 0;
            RestoreAuctioned();
            HitStop(0.2f);
            Shake(0.6f);
            Grant("boss" + zone);
            if (zone >= Zones)
            {
                state = State.Won;
                Sfx.Play(Sound.Win);
                if (debt == 0) Grant("debtfree");
                if (runTime < 720f) Grant("speed");
                EndRun(true);
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
            EndRun(false);
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

        /// <summary>Utrata fokusu karty: wycisza dzwiek i pauzuje gre.</summary>
        private void OnApplicationFocus(bool focused)
        {
            AudioListener.pause = !focused;
            if (!focused && !Paused && CanPause && ui != null) TogglePause();
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
                () => { Settings.Shake = !Settings.Shake; ShowPauseMenu(); },
                () => { Settings.MusicVolume = Settings.MusicVolume >= 0.99f ? 0f : Settings.MusicVolume + 0.1f; ShowPauseMenu(); },
                () => { Settings.SfxVolume = Settings.SfxVolume >= 0.99f ? 0f : Settings.SfxVolume + 0.1f; ShowPauseMenu(); });
        }

        private void Update()
        {
            if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P)) && (CanPause || Paused)) TogglePause();
            if (Paused) return;
            Time.timeScale = (Time.unscaledTime < hitStopUntil ? 0.05f : 1f) * DebugTimeScale;
            if (auctioned.Count > 0 && state == State.Playing)
            {
                auctionReturnTimer -= Time.deltaTime;
                if (auctionReturnTimer <= 0) RestoreAuctioned();
            }

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
                if (Input.GetKeyDown(KeyCode.Alpha1)) { Meta.Selected = Weapon.Pistol; GoToTitleRefresh(); }
                if (Input.GetKeyDown(KeyCode.Alpha2) && Meta.IsWeaponUnlocked(Weapon.Shotgun)) { Meta.Selected = Weapon.Shotgun; GoToTitleRefresh(); }
                if (Input.GetKeyDown(KeyCode.Alpha3) && Meta.IsWeaponUnlocked(Weapon.Laser)) { Meta.Selected = Weapon.Laser; GoToTitleRefresh(); }
                return;
            }
            if (state == State.ZoneIntro || state == State.Playing || state == State.Clearing || state == State.Shop) runTime += Time.deltaTime;
            if (state == State.Playing || state == State.Clearing) CheckAchievements();
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
                        noHitWaves = hitThisWave ? 0 : noHitWaves + 1;
                        if (noHitWaves >= 3) Grant("untouchable");
                        if (debt > 0)
                        {
                            debt = Mathf.CeilToInt(debt * (1f + InterestRate));
                            ui.Toast("INTEREST: DEBT IS NOW " + debt);
                            if (!Meta.TutorialDone && !debtHintShown)
                            {
                                debtHintShown = true;
                                ui.Toast("DEBT GROWS 20% EVERY WAVE. PAY IT IN THE SHOP BEFORE THE COLLECTOR COMES (WAVE 5)", 7f);
                            }
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
                    if (Input.GetKeyDown(KeyCode.T)) GoToTitle();
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
            if (!Meta.TutorialDone)
                ui.Toast("EVERY GAIN HAS A PRICE THAT LASTS THE WHOLE RUN. NO COINS? BUY ON CREDIT - BUT DEBT GROWS", 9f);
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
            Meta.TutorialDone = true;
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
