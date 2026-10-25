using System.Collections.Generic;
using UnityEngine;

namespace ZeroHour.Tower
{
    // Builds the whole game from code (no scene setup needed) and spawns falling floors.
    public class TowerGame : MonoBehaviour
    {
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

        [SerializeField]
        private float fallSpeed = 3f;

        [Header("Waves")]
        [Tooltip("Seconds between waves of falling floors.")]
        [SerializeField]
        private float spawnInterval = 1.8f;

        [SerializeField]
        private int minFloorsPerWave = 2;

        [SerializeField]
        private int maxFloorsPerWave = 3;

        [Tooltip("Minimum horizontal distance between floors in a wave, in floor widths.")]
        [SerializeField]
        private float waveSpacing = 1.5f;

        [Tooltip("Screen height fraction (from bottom) where the camera keeps the tower top.")]
        [SerializeField]
        private float towerTopScreenFraction = 0.4f;

        [SerializeField]
        private float cameraFollowSpeed = 3f;

        private readonly CityStats stats = new CityStats();
        private TowerHud hud;
        private Sprite squareSprite;
        private float spawnTimer;
        private float baseY;
        private float startCameraY;

        public Camera Camera { get; private set; }
        public TowerController Tower { get; private set; }
        public float PlayWidth => playWidth;
        public float BottomY => Camera.transform.position.y - Camera.orthographicSize;
        public float TopY => Camera.transform.position.y + Camera.orthographicSize;
        public float BaseY => baseY;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreateInstance()
        {
            new GameObject(nameof(TowerGame)).AddComponent<TowerGame>();
        }

        private void Awake()
        {
            Camera = Camera.main;
            Camera.orthographic = true;
            Camera.backgroundColor = new Color(0.55f, 0.78f, 0.95f);
            FitCamera();
            startCameraY = Camera.transform.position.y;
            baseY = BottomY + baseMargin;

            squareSprite = CreateSquareSprite(32, 3);

            Tower = new GameObject("Tower").AddComponent<TowerController>();
            Tower.transform.position = new Vector3(0f, BaseY, 0f);
            Tower.Init(this, squareSprite, platformWidth);

            hud = gameObject.AddComponent<TowerHud>();
        }

        private void Update()
        {
            FitCamera();

            stats.Recalculate(Tower.Floors);
            stats.Tick(Time.deltaTime);
            foreach (FallingFloor floor in Tower.Floors)
                floor.SetPowered(stats.IsPowered);
            hud.Refresh(stats, Tower.Stability);

            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f)
            {
                spawnTimer = spawnInterval;
                SpawnWave();
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

        // A row of floors at the same height; the player can catch only one of them.
        private void SpawnWave()
        {
            int count = Random.Range(minFloorsPerWave, maxFloorsPerWave + 1);
            float halfRange = (playWidth - floorSize) / 2f;
            float minDistance = waveSpacing * floorSize;

            var types = new List<FloorType>(FloorType.All);
            var positionsX = new List<float>();
            for (int i = 0; i < count; i++)
            {
                if (!TryPickWaveX(positionsX, halfRange, minDistance, out float x))
                    break;
                positionsX.Add(x);

                // Different types while there are unused ones left.
                if (types.Count == 0)
                    types.AddRange(FloorType.All);
                int typeIndex = Random.Range(0, types.Count);
                FloorType type = types[typeIndex];
                types.RemoveAt(typeIndex);

                SpawnFloor(new Vector3(x, TopY + floorSize, 0f), type);
            }
        }

        private static bool TryPickWaveX(List<float> takenX, float halfRange, float minDistance, out float x)
        {
            for (int attempt = 0; attempt < 30; attempt++)
            {
                x = Random.Range(-halfRange, halfRange);
                bool isFree = true;
                foreach (float other in takenX)
                    isFree &= Mathf.Abs(other - x) >= minDistance;
                if (isFree)
                    return true;
            }

            x = 0f;
            return false;
        }

        private void SpawnFloor(Vector3 position, FloorType type)
        {
            var floor = new GameObject("Floor").AddComponent<FallingFloor>();
            floor.transform.position = position;
            floor.Init(this, type, squareSprite, floorSize, fallSpeed);
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
