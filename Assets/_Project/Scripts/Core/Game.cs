using UnityEngine;
using UnityEngine.SceneManagement;

namespace PayWithPixels
{
    /// <summary>Caly stan runu: fale, monety, sklep, smierc. Wszystko budowane w kodzie (patrz Bootstrap).</summary>
    public class Game : MonoBehaviour
    {
        public enum State { Title, Playing, Clearing, Shop, Dead, Won }

        public const float OrthoSize = 5.625f; // 180 px / 16 PPU / 2
        public static readonly Vector2 ArenaHalf = new Vector2(9.5f, 5.2f);

        public static Game Instance { get; private set; }

        public State state;
        public int wave;
        public int coins;
        public RunStats stats;
        public Player player;
        public TwoColorDisplay display;
        private Camera cam;
        private GameUI ui;
        private Transform world;

        private int spawnLeft;
        private float spawnTimer;
        private float clearTimer;
        private float flickerClock;

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
            foreach (var c in Coin.All.ToArray()) if (c != null) Destroy(c.gameObject);
            if (player != null) Destroy(player.gameObject);

            Palette.Reset();
            flickerClock = 0;
            stats = new RunStats();
            wave = 0;
            coins = 0;
            ApplyPrices();
            player = Player.Create(world);
            ui.HideTitle();
            Sfx.StartMusic();
            ui.HideDeath();
            ui.HideShop();
            StartWave();
        }

        public void ApplyPrices()
        {
            display.SetPixelLevel(stats.pixelStacks);
            display.SetFog(stats.FogRadius);
        }

        private void StartWave()
        {
            wave++;
            Palette.SetIndex((wave - 1) / 2);
            state = State.Playing;
            spawnLeft = IsBossWave ? 6 : 4 + wave * 2;
            spawnTimer = 0.5f;
            if (IsBossWave)
            {
                Enemy.SpawnBoss(world, new Vector2(0, ArenaHalf.y - 1.5f));
                Sfx.Play(Sound.Boss);
            }
        }

        public const int BossWave = 8;
        public bool IsBossWave => wave == BossWave;

        public void AddCoins(int n)
        {
            coins += n;
            Sfx.Play(Sound.Coin);
        }

        public void OnBossKilled()
        {
            foreach (var e in Enemy.All.ToArray()) if (e != null) Destroy(e.gameObject);
            foreach (var b in Bullet.All.ToArray()) if (b != null) Destroy(b.gameObject);
            state = State.Won;
            Sfx.Play(Sound.Win);
            ui.ShowDeath(wave, StartRun, true);
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
            ui.ShowDeath(wave, StartRun);
        }

        private void Update()
        {
            if (player != null) display.SetFogCenter(player.transform.position);
            if (player != null && state != State.Title) ui.SetHud(player.hp, stats.maxHp, wave, coins);

            if (Input.GetKeyDown(KeyCode.M)) Sfx.ToggleMute();
            if (state == State.Title)
            {
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) StartRun();
                return;
            }
            UpdateFlicker();

            switch (state)
            {
                case State.Playing:
                    UpdateSpawner();
                    if (spawnLeft <= 0 && Enemy.All.Count == 0 && !IsBossWave)
                    {
                        state = State.Clearing;
                        clearTimer = 1.2f;
                    }
                    break;
                case State.Clearing:
                    clearTimer -= Time.deltaTime;
                    if (clearTimer <= 0) OpenShop();
                    break;
                case State.Won:
                case State.Dead:
                    if (Input.GetKeyDown(KeyCode.R)) StartRun();
                    break;
            }
        }

        private void UpdateFlicker()
        {
            float dur = stats.FlickerDuration;
            bool active = dur > 0 && (state == State.Playing || state == State.Clearing);
            if (!active) { Palette.Inverted = false; flickerClock = 0; return; }
            const float period = 8f;
            flickerClock = (flickerClock + Time.deltaTime) % (period + dur);
            Palette.Inverted = flickerClock > period;
        }

        private void UpdateSpawner()
        {
            if (spawnLeft <= 0) return;
            spawnTimer -= Time.deltaTime;
            if (spawnTimer > 0) return;
            spawnTimer = Mathf.Max(0.35f, 0.8f - wave * 0.04f);
            spawnLeft--;

            if (IsBossWave) { Enemy.Spawn(world, EnemyType.Chaser, PickSpawnPoint(), wave); spawnTimer = 4f; return; }
            EnemyType type = EnemyType.Chaser;
            float r = Random.value;
            if (wave >= 4 && r < 0.15f) type = EnemyType.Tank;
            else if (wave >= 2 && r < 0.45f) type = EnemyType.Shooter;
            Enemy.Spawn(world, type, PickSpawnPoint(), wave);
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

            state = State.Shop;
            ui.ShowShop(Catalog.Roll(3, wave, stats), coins, Buy, CloseShop);
        }

        private void Buy(Offer o)
        {
            if (o.sell) coins += o.reward;
            else
            {
                if (coins < o.cost) return;
                coins -= o.cost;
                o.gain.apply(stats, player);
            }
            Sfx.Play(Sound.Buy);
            o.price.apply(stats);
            ApplyPrices();
            CloseShop();
        }

        private void CloseShop()
        {
            ui.HideShop();
            StartWave();
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
