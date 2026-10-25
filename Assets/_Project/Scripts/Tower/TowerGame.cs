using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace ZeroHour.Tower
{
    // Builds the whole game from code (no scene setup needed) and spawns falling floors.
    public class TowerGame : MonoBehaviour
    {
        public static readonly Color SkyColor = new Color(0.55f, 0.78f, 0.95f);

        [SerializeField]
        private float playWidth = 6f;

        [SerializeField]
        private float minHalfHeight = 5f;

        [SerializeField]
        private float baseMargin = 1.5f;

        [SerializeField]
        private float platformWidth = 1.6f;

        [SerializeField]
        private float floorSize = 1f;

        [Header("Spawning")]
        [Tooltip("Minimum horizontal distance from the previous floor, in floor widths.")]
        [SerializeField]
        private float spawnSpacing = 1.5f;

        [Tooltip("Screen height fraction (from bottom) where the camera keeps the tower top.")]
        [SerializeField]
        private float towerTopScreenFraction = 0.4f;

        [SerializeField]
        private float cameraFollowSpeed = 3f;

        [Header("Round")]
        [SerializeField]
        private int scorePerFloor = 10;

        [Tooltip("Seconds after the round ends before a tap restarts, so a held finger doesn't skip the end screen.")]
        [SerializeField]
        private float restartDelay = 0.7f;

        private readonly List<FallingFloor> spawnedFloors = new List<FallingFloor>();
        private TowerRuleset ruleset;
        private CityStats stats;
        private TowerHud hud;
        private Sprite squareSprite;
        private float spawnTimer;
        private float? lastSpawnX;
        private FloorType lastSpawnType;
        private float baseY;
        private float startCameraY;
        private float timeLeft;
        private bool isGameOver;
        private float gameOverTime;

        public Camera Camera { get; private set; }
        public TowerController Tower { get; private set; }
        public float PlayWidth => playWidth;
        public float BottomY => Camera.transform.position.y - Camera.orthographicSize;
        public float TopY => Camera.transform.position.y + Camera.orthographicSize;
        public float BaseY => baseY;
        public TowerRuleset Ruleset => ruleset;

        public static TowerGame Create(TowerRuleset ruleset)
        {
            // Inactive until the ruleset is set, so Awake already sees it.
            var gameObject = new GameObject(nameof(TowerGame));
            gameObject.SetActive(false);
            var game = gameObject.AddComponent<TowerGame>();
            game.ruleset = ruleset;
            gameObject.SetActive(true);
            return game;
        }

        private void Awake()
        {
            Camera = Camera.main;
            Camera.orthographic = true;
            Camera.backgroundColor = SkyColor;
            FitCamera();
            startCameraY = Camera.transform.position.y;
            baseY = BottomY + baseMargin;

            squareSprite = CreateSquareSprite(32, 3);
            hud = gameObject.AddComponent<TowerHud>();
            if (ruleset.ShowMenuButton)
                hud.AddMenuButton();
            StartRound();
        }

        // Restart rebuilds the game objects instead of reloading the scene.
        private void StartRound()
        {
            foreach (FallingFloor floor in spawnedFloors)
            {
                if (floor)
                {
                    Destroy(floor.gameObject);
                }
            }
            spawnedFloors.Clear();
            if (Tower)
            {
                Destroy(Tower.gameObject);
            }

            Vector3 cameraPosition = Camera.transform.position;
            cameraPosition.y = startCameraY;
            Camera.transform.position = cameraPosition;

            Tower = new GameObject("Tower").AddComponent<TowerController>();
            Tower.transform.position = new Vector3(0f, BaseY, 0f);
            Tower.Init(this, squareSprite, platformWidth);

            stats = new CityStats();
            timeLeft = ruleset.RoundDuration;
            spawnTimer = 0f;
            lastSpawnX = null;
            lastSpawnType = null;
            isGameOver = false;
            hud.HideGameOver();
        }

        private void EndRound()
        {
            isGameOver = true;
            gameOverTime = Time.time;
            int score = stats.Height * scorePerFloor + Mathf.FloorToInt(stats.Money);
            hud.ShowGameOver(stats.Height, stats.Money, score);
        }

        private void Update()
        {
            FitCamera();

            if (isGameOver)
            {
                if (Time.time - gameOverTime >= restartDelay && TryGetTap(out Vector2 tapPosition))
                {
                    // Checked on press like the restart, so the restart can't hide the button first.
                    if (hud.IsOverMenuButton(tapPosition))
                        SceneManager.LoadScene(ZeroHour.GameBootstrap.VersionSelectScene);
                    else
                        StartRound();
                }
                return;
            }

            stats.Recalculate(Tower.Floors);
            stats.Tick(Time.deltaTime);
            foreach (var floor in Tower.Floors)
                floor.SetPowered(stats.IsPowered);

            timeLeft -= Time.deltaTime;
            hud.Refresh(stats, Tower.Stability, timeLeft);
            if (timeLeft <= 0f)
            {
                EndRound();
                return;
            }

            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f)
            {
                spawnTimer = ruleset.SpawnInterval(stats.Height);
                SpawnNextFloor();
            }
        }

        private void FitCamera()
        {
            Camera.orthographicSize = Mathf.Max(playWidth / 2f / Camera.aspect, minHalfHeight);
            if (Tower == null)
                return;

            // Keep the tower top at a fixed screen height, leaving free space above it.
            float size = Camera.orthographicSize;
            float targetY = Mathf.Max(startCameraY, Tower.TopY + size - 2f * size * towerTopScreenFraction);
            Vector3 cameraPosition = Camera.transform.position;
            cameraPosition.y = Mathf.Lerp(cameraPosition.y, targetY, 1f - Mathf.Exp(-cameraFollowSpeed * Time.deltaTime));
            Camera.transform.position = cameraPosition;
        }

        // One floor at a time, away from the previous one and of a different type,
        // so a few floors at different heights are in the air for the player to choose from.
        private void SpawnNextFloor()
        {
            float halfRange = (playWidth - floorSize) / 2f;
            float minDistance = spawnSpacing * floorSize;
            float x = Random.Range(-halfRange, halfRange);
            for (int attempt = 0; attempt < 30 && lastSpawnX.HasValue && Mathf.Abs(x - lastSpawnX.Value) < minDistance; attempt++)
                x = Random.Range(-halfRange, halfRange);

            FloorType type;
            do
                type = FloorType.GetRandom();
            while (type == lastSpawnType);

            lastSpawnX = x;
            lastSpawnType = type;
            SpawnFloor(new Vector3(x, TopY + floorSize, 0f), type);
        }

        private void SpawnFloor(Vector3 position, FloorType type)
        {
            var floor = new GameObject("Floor").AddComponent<FallingFloor>();
            floor.transform.position = position;
            floor.Init(this, type, squareSprite, floorSize, ruleset.FallSpeed(stats.Height));
            spawnedFloors.Add(floor);
        }

        private static bool TryGetTap(out Vector2 screenPosition)
        {
            var touchscreen = Touchscreen.current;
            if (touchscreen != null && touchscreen.primaryTouch.press.wasPressedThisFrame)
            {
                screenPosition = touchscreen.primaryTouch.position.ReadValue();
                return true;
            }
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                screenPosition = Mouse.current.position.ReadValue();
                return true;
            }
            screenPosition = default;
            return false;
        }

        // White square with a darker border; 9-sliced so the border keeps its width at any size.
        private static Sprite CreateSquareSprite(int resolution, int border)
        {
            var texture = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            var pixels = new Color32[resolution * resolution];
            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    bool isBorder = x < border || y < border || x >= resolution - border || y >= resolution - border;
                    byte value = isBorder ? (byte)140 : (byte)255;
                    pixels[y * resolution + x] = new Color32(value, value, value, 255);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();
            var spriteBorder = new Vector4(border, border, border, border);
            return Sprite.Create(texture, new Rect(0f, 0f, resolution, resolution), new Vector2(0.5f, 0.5f), resolution, 0, SpriteMeshType.FullRect, spriteBorder);
        }
    }
}
