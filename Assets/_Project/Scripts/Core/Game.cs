using UnityEngine;
using UnityEngine.SceneManagement;

namespace PayWithPixels
{
    /// <summary>Caly stan runu: fale, monety, sklep, smierc. Wszystko budowane w kodzie (patrz Bootstrap).</summary>
    public class Game : MonoBehaviour
    {
        public enum State { Playing, Clearing, Shop, Dead }

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
            world = new GameObject("World").transform;
            world.SetParent(transform, false);
            BuildArena();
            ui = GameUI.Create(transform);

            StartRun();
        }

        private void BuildArena()
        {
            var floor = Art.Square("tile_floor", 16);
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
            stats = new RunStats();
            wave = 0;
            coins = 0;
            ApplyPrices();
            player = Player.Create(world);
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
            state = State.Playing;
            spawnLeft = 4 + wave * 2;
            spawnTimer = 0.5f;
        }

        public void AddCoins(int n) => coins += n;

        public void SpawnCoin(Vector2 pos, int count)
        {
            for (int i = 0; i < count; i++)
                Coin.Spawn(world, pos + Random.insideUnitCircle * 0.4f);
        }

        public void OnPlayerDied()
        {
            state = State.Dead;
            ui.ShowDeath(wave, StartRun);
        }

        private void Update()
        {
            if (player != null) display.SetFogCenter(player.transform.position);
            if (player != null) ui.SetHud(player.hp, stats.maxHp, wave, coins);

            switch (state)
            {
                case State.Playing:
                    UpdateSpawner();
                    if (spawnLeft <= 0 && Enemy.All.Count == 0)
                    {
                        state = State.Clearing;
                        clearTimer = 1.2f;
                    }
                    break;
                case State.Clearing:
                    clearTimer -= Time.deltaTime;
                    if (clearTimer <= 0) OpenShop();
                    break;
                case State.Dead:
                    if (Input.GetKeyDown(KeyCode.R)) StartRun();
                    break;
            }
        }

        private void UpdateSpawner()
        {
            if (spawnLeft <= 0) return;
            spawnTimer -= Time.deltaTime;
            if (spawnTimer > 0) return;
            spawnTimer = Mathf.Max(0.35f, 0.8f - wave * 0.04f);
            spawnLeft--;

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
            ui.ShowShop(Catalog.Roll(3, wave), coins, Buy, CloseShop);
        }

        private void Buy(Offer o)
        {
            if (coins < o.cost) return;
            coins -= o.cost;
            o.gain.apply(stats, player);
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
