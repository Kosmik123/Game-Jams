using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PayWithPixels
{
    /// <summary>Caly stan runu: strefy, fale, monety, dlug, sklep, smierc. Wszystko budowane w kodzie (patrz Bootstrap).</summary>
    public class Game : MonoBehaviour
    {
        public enum State { Title, ZoneIntro, Playing, Clearing, Shop, Dead, Won, Story }

        public const float OrthoSize = TwoColorDisplay.BaseHeight / 16f / 2f; // px / 16 PPU / 2 (480x270 -> 8.4375)
        public static readonly Vector2 ArenaHalf = new Vector2(14.5f, 7.4f);  // ramka arena_bg: x 8..472, y 16..254 (pas HUD y 0-13 pusty)
        /// <summary>Przesuniecie kamery/tla; 0, bo arena_bg 480x270 ma pas HUD wbudowany u gory.</summary>
        public const float HudOffsetY = 0f;

        public const int Zones = 3;
        public const int WavesPerZone = 5;           // + boss strefy
        public const int MaxDebt = 20;

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
        private bool firstShopPending;
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
            string text = (won ? "YOU HAVE BEEN SOLD.\n" : "PAID IN FULL.\nZONE " + zone + ", WAVE " + wave + "\n") +
                "TOTAL PRICE PAID: " + TotalPricePaid + "\n" +
                "KILLS " + kills + "    COINS EARNED " + coinsEarned + "    TIME " + (t / 60) + ":" + (t % 60).ToString("00") +
                "\nCONTRACTS SIGNED " + contractsSigned +
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
            Online.Submit(score);
            boardPromptAt = Online.IsAvailable && score > 0 ? Time.unscaledTime : -1f;
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
        /// <summary>Wrogowie rosna z fala (co 3 fale +10% HP) i ze strefa (2: +15%, 3: +30% HP oraz predkosci).</summary>
        private float ZoneScale => zone >= 3 ? 1.2f : zone == 2 ? 1.15f : 1f;
        public float EnemyHpMul => (Ascension >= 2 ? 1.25f : 1f) * ZoneScale * (1f + 0.1f * (wave / 4));
        public float EnemySpeedMul => (Ascension >= 4 ? 1.15f : 1f) * enemySpeedContractMul * ZoneScale;
        public float BossHpMul => (Ascension >= 5 ? 1.2f : 1f) * ContractBossHpMul;
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
        public int Difficulty => (zone - 1) * WavesPerZone + waveInZone;

        public static void Create()
        {
            new GameObject("Game").AddComponent<Game>();
        }

        private void Awake()
        {
            Instance = this;
            Online.Init();   // anonimowe logowanie do tablicy wynikow w tle; bez sieci gra dziala normalnie
#if !UNITY_WEBGL
            Application.targetFrameRate = 60;   // w WebGL zostaje requestAnimationFrame
#endif

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
            bg.transform.position = new Vector3(0, HudOffsetY, 0);
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

        private float boardPromptAt = -1f;
        private const float BoardPromptTimeout = 12f;

        /// <summary>Po zgonie/wygranej: gdy wynik zostal wyslany, otwiera tablice (z wpisaniem nazwy przy pierwszym razie). Blad lub timeout = tablica sie nie pojawia.</summary>
        private void UpdateBoardPrompt()
        {
            if (boardPromptAt < 0f) return;
            if (Online.State == Online.Status.Error || Time.unscaledTime - boardPromptAt > BoardPromptTimeout)
            {
                boardPromptAt = -1f;
                return;
            }
            if (Online.State != Online.Status.Ready || Online.Busy || Online.HasPendingScore) return;
            boardPromptAt = -1f;
            ui.ShowLeaderboard(null, Online.HasName == false);
        }

        private void GoToTitleRefresh()
        {
            ui.ShowTitle(StartRun, w => { Meta.Selected = w; GoToTitleRefresh(); },
                () => { Meta.Ascension = (Meta.Ascension + 1) % (Meta.AscensionUnlocked + 1); GoToTitleRefresh(); });
        }

        private void CleanupRun()
        {
            boardPromptAt = -1f;
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

        // ---- Kontrakty: nagroda teraz, maly druk po FineWaves falach ----
        private readonly List<ActiveContract> activeContracts = new List<ActiveContract>();
        private Contract shopContract;
        private int shopVisits, contractsSigned, soldGains, contractValue;

        public int TotalPricePaid
        {
            get
            {
                int stacks = 0;
                foreach (var kv in stats.priceCounts) stacks += kv.Value;
                return stacks + debt + soldGains + contractValue;
            }
        }

        public void ContractLoseGain()
        {
            var name = stats.StealRandomGain(out _);
            RefreshDrones();
            ui.Toast("FINE PRINT: YOU LOST " + (name ?? "NOTHING (YOU HAD NO GAINS)"));
        }

        public void ContractHeal(int bonusHp)
        {
            player.hp = stats.maxHp;
            if (bonusHp > 0) player.hp += bonusHp;
        }

        public void ContractFreeGain()
        {
            var pool = Catalog.Gains.FindAll(g => !g.consumable && stats.Level(g.name) < g.maxLevel && (g.available == null || g.available(stats)));
            if (pool.Count == 0) { coins += 20; return; }
            var gain = pool[Random.Range(0, pool.Count)];
            stats.levels[gain.name] = stats.Level(gain.name) + 1;
            stats.Recompute();
            gain.onBuy?.Invoke(stats, player);
            RefreshDrones();
            ui.Toast("CONTRACT GAIN: " + gain.name);
        }

        public void ContractAddPrice(string name)
        {
            var price = Catalog.Prices.Find(pr => pr.name == name);
            if (price == null) return;
            price.apply(stats);
            stats.AddPrice(name);
            stats.Recompute();
            ApplyPrices();
            if (player.hp > stats.maxHp) player.hp = stats.maxHp;
        }

        // ---- efekty kontraktow (czasowe cofaja sie same po N falach albo na koncu strefy) ----
        private class Temp { public int waves; public bool untilZoneEnd; public System.Action revert; }
        private readonly List<Temp> temps = new List<Temp>();
        private float coinMul = 1f, enemySpeedContractMul = 1f, nextShopMarkup = 1f, shopMarkup = 1f;
        private int bonusDrop;
        private bool bossBuff, freeRerolls;

        private void AddTemp(int waves, bool untilZoneEnd, System.Action revert) =>
            temps.Add(new Temp { waves = waves, untilZoneEnd = untilZoneEnd, revert = revert });

        private void ResetContractEffects()
        {
            temps.Clear();
            coinMul = enemySpeedContractMul = nextShopMarkup = shopMarkup = 1f;
            bonusDrop = 0;
            bossBuff = freeRerolls = false;
        }

        public float ContractEnemySpeedMul => enemySpeedContractMul;
        public float ContractBossHpMul => bossBuff ? 1.2f : 1f;

        public void ContractMaxHp(int n)
        {
            stats.bonusMaxHp += n;
            stats.Recompute();
            player.hp += n;
        }

        public void ContractCoinMul(float mul, int waves)
        {
            coinMul = mul;
            AddTemp(waves, false, () => coinMul = 1f);
        }

        public void ContractBonusDrop(int waves)
        {
            bonusDrop = 1;
            AddTemp(waves, false, () => bonusDrop = 0);
        }

        public void ContractEnemySpeed(float mul, int waves)
        {
            enemySpeedContractMul = mul;
            AddTemp(waves, false, () => enemySpeedContractMul = 1f);
        }

        public void ContractFireBoost(float mul, int waves)
        {
            stats.tempFireMul = mul;
            AddTemp(waves, false, () => stats.tempFireMul = 1f);
        }

        public void ContractFreeRerolls()
        {
            freeRerolls = true;
            AddTemp(0, true, () => freeRerolls = false);
        }

        public void ContractNextShopMarkup(float mul) => nextShopMarkup = mul;

        public void ContractBossBuff()
        {
            bossBuff = true;
            ui.Toast("THE NEXT BOSS HAS 20% MORE HP");
        }

        public void ContractShield()
        {
            var g = Catalog.Gains.Find(x => x.name == "SHIELD");
            if (g == null || stats.Level(g.name) >= g.maxLevel) { coins += 20; return; }
            stats.levels[g.name] = stats.Level(g.name) + 1;
            stats.Recompute();
            g.onBuy?.Invoke(stats, player);
            player.RefillShield();
        }

        /// <summary>Cena czasowa: stacki wchodza od razu i znikaja po N falach albo na koncu strefy.</summary>
        public void ContractTempPrice(string name, int n, int waves, bool untilZoneEnd)
        {
            for (int i = 0; i < n; i++) ContractAddPrice(name);
            ui.Toast("FINE PRINT: " + name + " +" + n);
            AddTemp(waves, untilZoneEnd, () =>
            {
                var tmpPrice = Catalog.Prices.Find(pr => pr.name == name);
                if (tmpPrice != null) for (int i = 0; i < n; i++) tmpPrice.revert(stats);   // dziala dla kazdej ceny, nie tylko Mgly i Nog
                stats.priceCounts[name] = Mathf.Max(0, stats.PriceCount(name) - n);
                ApplyPrices();
                ui.Toast(name + " STACKS GONE");
            });
        }

        private void TickTemps(bool zoneEnd)
        {
            for (int i = temps.Count - 1; i >= 0; i--)
            {
                var t = temps[i];
                bool done = t.untilZoneEnd ? zoneEnd : (!zoneEnd && --t.waves <= 0);
                if (!done) continue;
                temps.RemoveAt(i);
                t.revert();
            }
        }

        private void TickContracts()
        {
            TickTemps(false);
            for (int i = activeContracts.Count - 1; i >= 0; i--)
            {
                var ac = activeContracts[i];
                if (--ac.wavesLeft > 0) continue;
                activeContracts.RemoveAt(i);
                ui.Toast("FINE PRINT KICKS IN: " + ac.contract.finePrint, 5f);
                ac.contract.later(this);
                Sfx.Play(Sound.Hurt);
                Grant("fineprint");
            }
        }

        private void SignContract()
        {
            if (shopContract == null || state != State.Shop) return;
            var c = shopContract;
            shopContract = null;
            contractsSigned++;
            contractValue += c.value;
            activeContracts.Add(new ActiveContract { contract = c, wavesLeft = Contracts.FineWaves });
            c.now(this);
            Sfx.Play(Sound.Buy);
            ui.Toast("SIGNED: " + c.title + ". FINE PRINT IN " + Contracts.FineWaves + " WAVES: " + c.finePrint, 6f);
            if (contractsSigned >= 3) Grant("signed");
            RefreshShopUi();
        }

        private void StartRun()
        {
            activeContracts.Clear();
            ResetContractEffects();
            contractValue = 0;
            shopContract = null;
            shopVisits = contractsSigned = soldGains = 0;
            CleanupRun();
            Palette.Reset();
            flickerClock = 0;
            auctioned.Clear();
            runTime = 0;
            kills = 0;
            collectorMeetings = 0;
            coinsEarned = 0;
            noHitWaves = 0;
            hitThisWave = false;
            newAchievements.Clear();
            firstShopPending = true;
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
            if (!introSeen && !Application.isBatchMode)
            {
                introSeen = true;   // intro raz na sesje strony; Esc/klik pomija
                StartStory(IntroSlides, BeginZone);
            }
            else BeginZone();
        }

        // ---- fabula: intro (pomijalne) i linijki bossow ----
        private static bool introSeen;
        private static readonly string[][] IntroSlides =
        {
            new[] { "story_intro_1", "" },   // podpisy sa w grafice kadrow
            new[] { "story_intro_2", "" },
            new[] { "story_intro_3", "" },
            new[] { "story_intro_4", "" },
        };
        private static readonly string[] BossLines =
        {
            "HALT. THE ROAD IS NOT FREE.",
            "YOU OWE ME. SO WE CAN TALK.",
            "GOING ONCE... GOING TWICE... SOLD!",
        };
        private int storyIndex;
        private string[][] storySlides;
        private System.Action storyDone;
        private int storyFrame;

        private void StartStory(string[][] slides, System.Action done)
        {
            storyFrame = Time.frameCount;
            storySlides = slides;
            storyDone = done;
            storyIndex = 0;
            state = State.Story;
            ui.ShowStory(slides[0][0], slides[0][1], 1, slides.Length);
        }

        private void UpdateStory()
        {
            if (Time.frameCount <= storyFrame + 1) return;   // klik, ktory wystartowal run, nie przewija slajdu
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Backspace)) storyIndex = storySlides.Length;
            else if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0)) storyIndex++;
            else return;

            if (storyIndex < storySlides.Length)
            {
                ui.ShowStory(storySlides[storyIndex][0], storySlides[storyIndex][1], storyIndex + 1, storySlides.Length);
                return;
            }
            ui.HideStory();
            var done = storyDone;
            storyDone = null;
            done?.Invoke();
        }

        public void ApplyPrices()
        {
            display.SetPixelLevel(stats.pixelStacks);
            display.SetFog(stats.FogRadius);
        }

        private void BeginZone()
        {
            TickTemps(true);
            state = State.ZoneIntro;
            introTimer = 2.6f;
            waveInZone = 0;
            Palette.SetZone(zone, 1);
            Sfx.SetMusic(zone);
            Obstacles.Build(world, zone);
            ui.ShowZone(zone, ZoneNames[zone - 1]);
        }

        [Button]
        private void ShowDebugSpawnCounts()
        {
            int allWavesPerZone = (WavesPerZone + 1);
            for (int w = 0; w < 20; w++)
            {
                int zone = (w / allWavesPerZone) + 1;
                int waveInZone = w % allWavesPerZone + 1;
                int count = SpawnCount(zone, waveInZone, w);
                Debug.Log($"Zone: {zone}-{waveInZone} -> {count} enemies");
            }
        }


        /// <summary>Liczba wrogow w fali; x1.5 po powiekszeniu areny do 480x270 (2.25x powierzchni). Z1-1..3 lagodniej.</summary>
        private static int SpawnCount(int zone, int waveInZone, int wave)
        {
            int n = zone == 1
                ? (waveInZone <= 3 ? 5 : 8) + 2 * waveInZone
                : 8 + 2 * waveInZone + 4 * (zone - 1);
            return Mathf.RoundToInt((n + wave / 3) * 1.5f);
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
            spawnLeft = IsBossWave ? 9 : SpawnCount(zone, waveInZone, wave);
            collectorAtLeft = -1;
            spawnTimer = 0.5f;
            player.RefillShield();
            freezeReady = stats.freeze > 0;

            if (IsBossWave)
            {
                Enemy.SpawnBoss(world, new Vector2(0, ArenaHalf.y - 1.5f), zone);
                ui.ShowBossLine(BossLines[Mathf.Clamp(zone - 1, 0, BossLines.Length - 1)]);
                bossBuff = false;
                Sfx.Play(Sound.Boss);
            }
            else if (CollectorComes())
                collectorAtLeft = Random.Range(0, Mathf.Max(1, spawnLeft));   // Poborca wchodzi po losowej liczbie zespawnowanych wrogow
        }

        private int collectorAtLeft = -1;   // spawnLeft, przy ktorym wchodzi Poborca (-1 = w tej fali nie wchodzi)

        private void SpawnCollectorIfDue()
        {
            if (collectorAtLeft < 0 || spawnLeft > collectorAtLeft) return;
            collectorAtLeft = -1;
            Enemy.SpawnCollector(world, PickSpawnPoint());
            ui.Toast("THE COLLECTOR COMES FOR YOUR DEBT");
        }

        /// <summary>
        /// Poborca: od 2. fali strefy szansa = dlug / MaxDebt (min. 10% przy dlugu > 0), maks. jeden naraz;
        /// pewny w ostatniej zwyklej fali strefy, gdy dlug >= polowy limitu.
        /// </summary>
        private bool CollectorComes()
        {
            if (debt <= 0 || waveInZone < 2) return false;
            foreach (var e in Enemy.All) if (e != null && e.type == EnemyType.Collector) return false;
            if (waveInZone == WavesPerZone && debt >= MaxDebt / 2) return true;
            return Random.value < Mathf.Clamp(debt / (float)MaxDebt, 0.1f, 1f);
        }

        public void AddCoins(int n)
        {
            float v = n * coinMul;   // bez bonusu stref 2-3 (byl +25%): dochod rosl szybciej niz ceny
            n = Mathf.FloorToInt(v) + (Random.value < v - Mathf.Floor(v) ? 1 : 0);
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
            foreach (var e in Enemy.All.ToArray()) if (e != null) e.ReturnHeldGain();   // boss kasuje wrogow: uciekajacy Poborca oddaje zysk
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
            ui.Toast("COLLECTOR DOWN");
        }

        /// <summary>Poborca dotknal gracza: zabiera losowy zysk, zostawia cene.</summary>
        public string OnCollectorTouch(out int level)
        {
            string taken = stats.StealRandomGain(out level);
            RefreshDrones();
            if (player.hp > stats.maxHp) player.hp = stats.maxHp;
            ui.Toast(taken != null ? "THE COLLECTOR TOOK: " + taken + " - KILL IT BEFORE IT ESCAPES" : "THE COLLECTOR FOUND NOTHING TO TAKE");
            return taken;
        }

        /// <summary>Poborca uciekl z lupem za brzeg areny: zysk zostaje stracony.</summary>
        public void OnCollectorEscaped(string gain)
        {
            if (gain != null) ui.Toast("THE COLLECTOR ESCAPED WITH: " + gain);
        }

        /// <summary>Poborca zabity w trakcie ucieczki: zabrany zysk wraca do gracza.</summary>
        public void RestoreGain(string gain, int level)
        {
            stats.RestoreGain(gain, level);
            RefreshDrones();
            ui.Toast("BONUS RECOVERED: " + gain);
        }

        public int collectorMeetings;   // ile razy Poborca juz sie pojawil w tym runie: kazdy kolejny ma wiecej HP

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
            for (int i = 0; i < bonusDrop; i++) Coin.Spawn(world, pos + Random.insideUnitCircle * 0.6f);
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
            Vector3 p = new Vector3(0, HudOffsetY, -10);
            if (Settings.Shake && shake > 0.01f && !Paused)
            {
                // przesuniecie przyciete do 1/16 j. = 1 piksel RT, zeby obraz nie mazal
                p.x = Mathf.Round(Random.Range(-shake, shake) * 16f) / 16f;
                p.y += Mathf.Round(Random.Range(-shake, shake) * 16f) / 16f;
            }
            shake = Mathf.MoveTowards(shake, 0f, 1.6f * Time.unscaledDeltaTime);
            cam.transform.position = p;
        }

        [SerializeField]
        private bool shouldPauseOnDefocus;
        /// <summary>Utrata fokusu karty: wycisza dzwiek i pauzuje gre.</summary>
        private void OnApplicationFocus(bool focused)
        {
            if (shouldPauseOnDefocus == false && focused == false)
                return;

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
            ui.ShowPause(
                onResume: TogglePause,
                onRestart: () => { TogglePause(); StartRun(); },
                onVolume: () => { Settings.Volume = Settings.Volume >= 0.99f ? 0f : Settings.Volume + 0.1f; ShowPauseMenu(); },
                onFlicker: () => { Settings.ReduceFlicker = !Settings.ReduceFlicker; ShowPauseMenu(); },
                onShake: () => { Settings.Shake = !Settings.Shake; ShowPauseMenu(); },
                onMusic: () => { Settings.MusicVolume = Settings.MusicVolume >= 0.99f ? 0f : Settings.MusicVolume + 0.1f; ShowPauseMenu(); },
                onSfx: () => { Settings.SfxVolume = Settings.SfxVolume >= 0.99f ? 0f : Settings.SfxVolume + 0.1f; ShowPauseMenu(); },
                onReset: () => { TogglePause(); GoToTitle(); },
                resetLabel: "RETURN TO TITLE",
                onExport: () => { Save.ShowExport(); ui.Toast("SAVE CODE COPIED", 3f); },
                onImport: () =>
                {
                    bool ok = Save.AskImport();
                    ShowPauseMenu();
                    ui.Toast(ok ? "SAVE LOADED" : "INVALID SAVE CODE", 3f);
                });
        }

        private void Update()
        {
            Sfx.ApplyVolumes();   // SILENCE moze sie zmienic w kazdej chwili
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

            if (ui.OverlayOpen) return;   // tablica wynikow przejmuje klawiature (wpisywanie nazwy)
            if (Input.GetKeyDown(KeyCode.M)) Sfx.ToggleMute();
            if (state == State.Story) { UpdateStory(); return; }
            if (state == State.Title)
            {
                if (ui.OverlayOpen) return;   // tablica wynikow otwarta: Enter/Spacja nie startuja runu
                if (Input.GetKeyDown(KeyCode.L) && Online.IsAvailable) { ui.ShowLeaderboard(); return; }
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
            UpdateSilence();

            switch (state)
            {
                case State.ZoneIntro:
                    introTimer -= Time.deltaTime;
                    if (introTimer <= 0) StartWave();
                    break;
                case State.Playing:
                    if (!Frozen) Enemy.Separate(Time.deltaTime);
                    UpdateSpawner();
                    UpdateLingerFailsafe();
                    if (spawnLeft <= 0 && Enemy.All.Count == 0)
                    {
                        state = State.Clearing;
                        clearTimer = 1.2f;
                        TickContracts();
                        noHitWaves = hitThisWave ? 0 : noHitWaves + 1;
                        if (noHitWaves >= 3) Grant("untouchable");
                        if (debt > 0)
                        {
                            debt = Mathf.CeilToInt(debt * (1f + InterestRate));
                            ui.Toast("INTEREST: DEBT IS NOW " + debt);
                            if (!Meta.TutorialDone && !debtHintShown)
                            {
                                debtHintShown = true;
                                ui.Toast("DEBT GROWS 20% EVERY WAVE. THE COLLECTOR VISITS BIG DEBTS", 7f);
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
                    UpdateBoardPrompt();
                    if (ui.OverlayOpen) break;
                    if (Input.GetKeyDown(KeyCode.L)) { ui.ShowLeaderboard(); break; }
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
            else if (Input.GetKeyDown(KeyCode.C)) SignContract();
            else if (shopRedeem)
            {
                var opts = RedeemOptions();
                KeyCode[] rk = { KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6 };
                for (int i = 0; i < rk.Length && opts != null && i < opts.Count; i++)
                    if (Input.GetKeyDown(rk[i])) { Redeem(opts[i].Key); return; }
            }
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
            float period = 11f - 0.5f * stacks, duration = 2f;
            float prev = invertClock;
            invertClock += Time.deltaTime;
            invertClock %= (period + duration);
            ControlsInverted = invertClock > period;
            if (ControlsInverted && prev <= period) ui.Toast("CONTROLS INVERTED!");
        }

        /// <summary>Mnoznik glosnosci (0..1) dla ceny SILENCE: plynnie wycisza dzwiek i muzyke w losowych momentach fali.</summary>
        public float SilenceMul { get; private set; } = 1f;
        private float silenceTimer, silenceLeft;
        private bool silenceWasActive;

        private void UpdateSilence()
        {
            int stacks = stats.silenceStacks;
            bool active = stacks > 0 && (state == State.Playing || state == State.Clearing);
            float target = 1f;
            if (!active) { silenceWasActive = false; silenceLeft = 0; }
            else
            {
                if (!silenceWasActive) { silenceWasActive = true; silenceLeft = 0; silenceTimer = Random.Range(5f, stacks > 1 ? 10f : 12f); }
                if (silenceLeft > 0)
                {
                    silenceLeft -= Time.deltaTime;
                    target = 0f;
                    if (silenceLeft <= 0) silenceTimer = stacks > 1 ? Random.Range(5f, 10f) : Random.Range(6f, 12f);
                }
                else
                {
                    silenceTimer -= Time.deltaTime;
                    if (silenceTimer <= 0) { silenceLeft = 1.5f + 0.5f * stacks; target = 0f; ui.Toast("SILENCE!"); }
                }
            }
            SilenceMul = Mathf.MoveTowards(SilenceMul, target, Time.unscaledDeltaTime / 0.1f);
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
            spawnTimer = Mathf.Max(0.5f, 1.0f - Difficulty * 0.03f);
            spawnLeft--;
            SpawnCollectorIfDue();

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
                SpawnCollectorIfDue();
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

            Add(EnemyType.Chaser, zone == 1 ? 5f : 3f, 1);
            Add(EnemyType.Shooter, 3f, zone == 1 ? 2 : 1);
            Add(EnemyType.Tank, 1.2f, zone == 1 ? 4 : 3);
            if (zone >= 2)
            {
                Add(EnemyType.Dasher, 2.5f);
                Add(EnemyType.Swarm, 2f, 2);
                Add(EnemyType.Guard, 1.2f, 3);
            }
            if (zone >= 3)
            {
                Add(EnemyType.Siege, 1.4f);
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
                if (player != null && (p - (Vector2)player.transform.position).sqrMagnitude <= 16f) continue;
                bool crowded = false;
                foreach (var e in Enemy.All)
                    if (e != null && ((Vector2)e.transform.position - p).sqrMagnitude < 1.3f * 1.3f) { crowded = true; break; }
                if (!crowded) break;
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
            if (firstShopPending)
            {
                firstShopPending = false;
                var first = shopOffers.Find(o => !o.sell);
                if (first != null) first.free = true;   // pierwszy zysk w runie jest za darmo, cena (zmysl) nie
            }
            shopVisits++;
            shopMarkup = nextShopMarkup;
            nextShopMarkup = 1f;
            shopRedeem = OwnedPrices().Count > 0;                               // REDEEM w kazdym sklepie (1 na wizyte)
            shopContract = shopVisits % 2 == 0 ? Contracts.Roll() : null;       // kontrakt co 2. sklep
            ui.AnimateNextShop = true;
            if (!Meta.TutorialDone)
                ui.Toast("THE FIRST GAIN IS FREE - ITS PRICE IS NOT. EVERY PRICE LASTS THE WHOLE RUN. NO COINS? BUY ON CREDIT, BUT DEBT GROWS", 10f);
            rerolls = 0;
            purchases = 0;
            haggles = 0;
            RefreshShopUi();
        }

        private List<Offer> shopOffers;
        private int rerolls;
        private int purchases;   // zakupy w tej wizycie: kazdy kolejny +25%
        private int haggles;
        private int HaggleMax => 1 + stats.priceLock;

        // ---- REDEEM: zdejmij 1 stack wybranej ceny (co 3. sklep zamiast kontraktu) ----
        private bool shopRedeem;

        public int RedeemCost(string priceName) => 10 + 5 * stats.PriceCount(priceName);

        private List<string> OwnedPrices()
        {
            var list = new List<string>();
            foreach (var p in Catalog.Prices) if (stats.PriceCount(p.name) > 0) list.Add(p.name);
            return list;
        }

        private bool RemovePriceStack(string name)
        {
            var price = Catalog.Prices.Find(p => p.name == name);
            if (price == null || price.revert == null || stats.PriceCount(name) <= 0) return false;
            price.revert(stats);
            stats.priceCounts[name] = stats.PriceCount(name) - 1;
            stats.Recompute();
            ApplyPrices();
            return true;
        }

        private string RemoveRandomPriceStack()
        {
            var owned = OwnedPrices();
            if (owned.Count == 0) return null;
            string name = owned[Random.Range(0, owned.Count)];
            return RemovePriceStack(name) ? name : null;
        }

        private void Redeem(string name)
        {
            if (!shopRedeem || state != State.Shop) return;
            int cost = RedeemCost(name);
            if (coins < cost || !RemovePriceStack(name)) return;
            coins -= cost;
            shopRedeem = false;
            Sfx.Play(Sound.Buy);
            ui.Toast("REDEEMED: " + name + " -1 STACK");
            RefreshShopUi();
        }

        private int RerollCost => freeRerolls ? 0 : 3 + 2 * rerolls;

        /// <summary>Aktualny koszt oferty: +25% za kazdy wczesniejszy zakup w tej wizycie.</summary>
        public int CostOf(Offer o) => o.free ? 0 : Mathf.Max(1, Mathf.RoundToInt(Mathf.CeilToInt(o.cost * (1f + 0.25f * purchases) * shopMarkup * (1f + 0.2f * stats.priceyShopStacks)) * Catalog.PriceDiscount(o.price)));

        private List<KeyValuePair<string, int>> RedeemOptions()
        {
            if (!shopRedeem) return null;
            var list = new List<KeyValuePair<string, int>>();
            foreach (var name in OwnedPrices()) list.Add(new KeyValuePair<string, int>(name, RedeemCost(name)));
            return list;
        }

        private string DebtInfo()
        {
            if (debt <= 0) return null;
            return "+" + Mathf.RoundToInt(InterestRate * 100) + "% EVERY WAVE";   // bez terminu Poborcy: pojawia sie losowo wg dlugu
        }

        private void RefreshShopUi()
        {
            string pending = "";
            foreach (var ac in activeContracts) pending += (pending.Length > 0 ? ", " : "") + ac.contract.title + " (" + ac.wavesLeft + ")";
            ui.ShowShop(shopOffers, coins, debt, Buy, CloseShop, RerollCost, Reroll, haggles < HaggleMax, Haggle, PayDebt,
                shopContract == null ? null : shopContract.title + ": " + shopContract.reward + " / FINE PRINT: ???", SignContract,
                pending.Length > 0 ? "SIGNED: " + pending : null, DebtInfo(), RedeemOptions(), Redeem);
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
            if (haggles >= HaggleMax || coins < 1) return;
            coins -= 1;
            haggles++;
            foreach (var o in shopOffers)
                if (!o.sold)
                {
                    o.price = Catalog.Prices[Random.Range(0, Catalog.Prices.Count)];
                    if (o.sell) o.reward = Catalog.SellReward(wave, o.price);
                }
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
            if (o.sell) { coins += o.reward; soldGains++; }
            else
            {
                int c = CostOf(o);
                if (coins >= c) coins -= c;
                else
                {
                    debt += c - coins;   // brak monet: reszta na kredyt
                    coins = 0;
                }
                if (!o.free) purchases++;
                var g = o.gain;
                if (!g.consumable) stats.levels[g.name] = o.level;
                stats.Recompute();
                g.onBuy?.Invoke(stats, player);
                RefreshDrones();
                if (player.hp > stats.maxHp) player.hp = stats.maxHp;
            }
            Sfx.Play(Sound.Buy);
            for (int s = 0; s < o.stacks; s++)
            {
                if (s > 0 && o.price.available != null && !o.price.available(stats)) break;   // limity stackow (np. ZOOM IN)
                o.price.apply(stats);
                stats.AddPrice(o.price.name);
            }
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

        [SerializeField]
        private Sound debugSound;

        [Button]
        private void PlayDebugSound()
        {
            Sfx.Play(debugSound);
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
