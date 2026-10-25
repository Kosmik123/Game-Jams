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

        private const int PerfectFontSize = 70;
        private const float PerfectPopupHeight = 0.8f;
        private const float PerfectRise = 1f;
        private const float PerfectDuration = 0.8f;
        private const int IncomeFontSize = 48;
        private const float IncomePopupHeight = 0.3f;
        private const float IncomeRise = 0.8f;
        private const float IncomeDuration = 0.7f;
        private static readonly Color IncomeColor = new Color(0.45f, 1f, 0.45f);

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

        [Tooltip("With a fixed camera: how fast the tower sinks (or rises after a collapse) to keep its top in place.")]
        [SerializeField]
        private float towerSinkSpeed = 3f;

        [Header("Round")]
        [SerializeField]
        private int scorePerFloor = 10;

        [Tooltip("Post-jam tutorial hides after the first caught floor or after this many seconds.")]
        [SerializeField]
        private float tutorialDuration = 5f;

        [Tooltip("End-of-round shot of the whole tower (versions with ShowTowerShot).")]
        [SerializeField]
        private float towerShotDuration = 1.5f;

        [Tooltip("Share of the screen height (from the bottom) the tower may take in the end shot.")]
        [SerializeField]
        private float towerShotScreenShare = 2f / 3f;

        [Tooltip("Margin around the tower in the end shot, as a share of its size (split between both sides).")]
        [SerializeField]
        private float towerShotMargin = 0.1f;

        [Header("Juice")]
        [SerializeField]
        private float shakeDuration = 0.3f;

        [SerializeField]
        private float shakeAmplitude = 0.15f;

        [Tooltip("Seconds between \"+$\" texts from the roof.")]
        [SerializeField]
        private float incomePopupInterval = 1f;

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
        private float startBaseY;
        private float startCameraY;
        private float startCameraX;
        private float towerShotTime = -1f;
        private Vector3 towerShotStartPosition;
        private float towerShotStartSize;
        private float timeLeft;
        private float roundElapsed;
        private float shakeTimeLeft;
        private float incomePopupTimer;
        private float moneyAtLastPopup;
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
            startCameraX = Camera.transform.position.x;
            startBaseY = BottomY + baseMargin;
            baseY = startBaseY;

            squareSprite = CreateSquareSprite(32, 3);
            hud = gameObject.AddComponent<TowerHud>();
            hud.TimerWarningSeconds = ruleset.TimerWarningSeconds;
            if (ruleset.ShowTowerShot)
                hud.UseTopLayout();
            hud.PopulationLabel = ruleset.PopulationLabel;
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
            cameraPosition.x = startCameraX;
            Camera.transform.position = cameraPosition;
            baseY = startBaseY;
            towerShotTime = -1f;

            Tower = new GameObject("Tower").AddComponent<TowerController>();
            Tower.transform.position = new Vector3(0f, BaseY, 0f);
            Tower.Init(this, squareSprite, platformWidth);
            Tower.FloorLanded += OnFloorLanded;
            Tower.Collapsed += OnTowerCollapsed;

            stats = ruleset.CreateEconomy();
            timeLeft = ruleset.RoundDuration;
            roundElapsed = 0f;
            shakeTimeLeft = 0f;
            incomePopupTimer = 0f;
            moneyAtLastPopup = 0f;
            spawnTimer = 0f;
            lastSpawnX = null;
            lastSpawnType = null;
            isGameOver = false;
            hud.HideGameOver();
            if (ruleset.ShowTutorial)
                hud.ShowTutorial("Move finger to catch floors");
        }

        private void EndRound()
        {
            isGameOver = true;
            gameOverTime = Time.time;
            hud.HideTutorial();
            if (ruleset.ShowTowerShot)
            {
                towerShotTime = 0f;
                towerShotStartPosition = Camera.transform.position;
                towerShotStartSize = Camera.orthographicSize;
                Tower.SuppressSway = true;
                Tower.IsFrozen = true;
            }
            int score = stats.Height * scorePerFloor + Mathf.FloorToInt(stats.Money);
            string bestKey = ruleset.BestScoreKey;
            if (bestKey == null)
            {
                hud.ShowGameOver(stats.Height, stats.Money, score);
                return;
            }

            // The first round ever is a record too.
            bool isNewBest = !PlayerPrefs.HasKey(bestKey) || score > PlayerPrefs.GetInt(bestKey);
            if (isNewBest)
            {
                PlayerPrefs.SetInt(bestKey, score);
                PlayerPrefs.Save(); // WebGL: written to IndexedDB right away
            }
            hud.ShowGameOver(stats.Height, stats.Money, score, PlayerPrefs.GetInt(bestKey), isNewBest);
        }

        private void Update()
        {
            if (towerShotTime >= 0f)
                UpdateTowerShot();
            else
            {
                FitCamera();
                if (shakeTimeLeft > 0f)
                    UpdateShake();
            }

            if (isGameOver)
            {
                // Taps wait for the end shot to finish.
                float delay = towerShotTime >= 0f ? Mathf.Max(restartDelay, towerShotDuration) : restartDelay;
                if (Time.time - gameOverTime >= delay && TryGetTap(out Vector2 tapPosition))
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
            for (int i = 0; i < Tower.Floors.Count; i++)
                Tower.Floors[i].SetPowered(stats.IsFloorPowered(i));

            timeLeft -= Time.deltaTime;
            roundElapsed += Time.deltaTime;
            if (ruleset.ShowTutorial && (Tower.Floors.Count > 0 || roundElapsed >= tutorialDuration))
                hud.FadeOutTutorial();
            hud.Refresh(stats, Tower.Stability, timeLeft);
            if (timeLeft <= 0f)
            {
                EndRound();
                return;
            }

            if (ruleset.ShowJuice)
                UpdateIncomePopup();

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
            if (ruleset.FixedCamera)
            {
                // The camera stays put and the whole tower sinks instead, so falling floors
                // keep a steady speed on screen. Never higher than the starting position.
                float allowedTopY = startCameraY - size + 2f * size * towerTopScreenFraction;
                float targetBaseY = Mathf.Min(startBaseY, allowedTopY - Tower.LocalTop);
                baseY = Mathf.Lerp(baseY, targetBaseY, 1f - Mathf.Exp(-towerSinkSpeed * Time.deltaTime));
                return;
            }

            float targetY = Mathf.Max(startCameraY, Tower.TopY + size - 2f * size * towerTopScreenFraction);
            Vector3 cameraPosition = Camera.transform.position;
            cameraPosition.y = Mathf.Lerp(cameraPosition.y, targetY, 1f - Mathf.Exp(-cameraFollowSpeed * Time.deltaTime));
            Camera.transform.position = cameraPosition;
        }

        private void OnFloorLanded(FallingFloor floor, bool isPerfect)
        {
            stats.RegisterLanding(isPerfect);
            if (isPerfect && ruleset.ShowPerfectPopup)
            {
                int combo = stats.ComboMultiplier;
                WorldPopup.Show(hud, Camera, floor.transform, Vector3.up * PerfectPopupHeight,
                    combo > 1 ? $"PERFECT! x{combo}" : "PERFECT!", PerfectFontSize, Color.white, PerfectRise, PerfectDuration);
            }
            ChangeTime(ruleset.FloorTimeBonus(Tower.Floors.Count, isPerfect));
        }

        private void OnTowerCollapsed()
        {
            stats.RegisterCollapse();
            if (ruleset.ShowJuice)
                shakeTimeLeft = shakeDuration;
            ChangeTime(-ruleset.CollapseTimePenalty);
        }

        // Every second with income, "+$N" (money earned in that second) rises from the roof.
        private void UpdateIncomePopup()
        {
            incomePopupTimer += Time.deltaTime;
            if (incomePopupTimer < incomePopupInterval)
                return;
            incomePopupTimer -= incomePopupInterval;
            int earned = Mathf.RoundToInt(stats.Money - moneyAtLastPopup);
            moneyAtLastPopup = stats.Money;
            if (earned <= 0)
                return;
            var roof = new Vector3(Tower.TopOffset, Tower.LocalTop + IncomePopupHeight, 0f);
            WorldPopup.Show(hud, Camera, Tower.transform, roof, $"+${earned}", IncomeFontSize, IncomeColor, IncomeRise, IncomeDuration);
        }

        // Shakes the (still) camera around its start position, fading out, then puts it back exactly.
        private void UpdateShake()
        {
            shakeTimeLeft -= Time.deltaTime;
            var home = new Vector3(startCameraX, startCameraY, Camera.transform.position.z);
            if (shakeTimeLeft <= 0f)
            {
                shakeTimeLeft = 0f;
                Camera.transform.position = home;
                return;
            }
            float strength = shakeAmplitude * shakeTimeLeft / shakeDuration;
            Camera.transform.position = home + (Vector3)(Random.insideUnitCircle * strength);
        }

        // Bonus or penalty from the tower; only during the round.
        private void ChangeTime(float change)
        {
            if (isGameOver || change == 0f)
                return;
            float before = timeLeft;
            timeLeft = ruleset.ApplyTimeChange(timeLeft, change);
            hud.ShowTimeChange(timeLeft - before, change);
        }

        // Zooms out until the whole building, from the platform to the roof, fills the lower part of the
        // screen. The target is recomputed every frame, as floors still in the air may land.
        private void UpdateTowerShot()
        {
            towerShotTime += Time.deltaTime;
            float t = Mathf.Clamp01(towerShotTime / towerShotDuration);
            float eased = t * t * (3f - 2f * t);

            Tower.GetBounds(out float minX, out float maxX, out float bottomY, out float topY);
            float height = topY - bottomY;
            float marginY = height * towerShotMargin / 2f;
            float width = maxX - minX;
            float marginX = width * towerShotMargin / 2f;
            float aspect = Camera.aspect;

            float size = Mathf.Max(towerShotStartSize,
                (height + 2f * marginY) / (2f * towerShotScreenShare),
                (width + 2f * marginX) / 2f / aspect);

            // Move only as much as needed: the tower within the lower share of the screen, the start camera
            // spot kept when it already fits (no jump for a short tower).
            float upperY = bottomY - marginY + size;
            float lowerY = topY + marginY - size + 2f * size * towerShotScreenShare;
            float targetY = lowerY > upperY ? upperY : Mathf.Clamp(towerShotStartPosition.y, lowerY, upperY);
            float halfWidth = size * aspect;
            float lowerX = maxX + marginX - halfWidth;
            float upperX = minX - marginX + halfWidth;
            float targetX = lowerX > upperX ? (minX + maxX) / 2f : Mathf.Clamp(towerShotStartPosition.x, lowerX, upperX);

            Vector3 target = new Vector3(targetX, targetY, towerShotStartPosition.z);
            Camera.transform.position = Vector3.Lerp(towerShotStartPosition, target, eased);
            Camera.orthographicSize = Mathf.Lerp(towerShotStartSize, size, eased);
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
