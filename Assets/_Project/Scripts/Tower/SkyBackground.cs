using UnityEngine;

namespace ZeroHour.Tower
{
    // A sky behind everything that changes with the tower height: a vertical gradient (a 1x64 texture on a
    // sprite) and twinkling pixel stars. Both are children of the camera and rescaled to the view every
    // frame, so they follow zooms (the end shot) and window resizes.
    public class SkyBackground : MonoBehaviour
    {
        private const int GradientSteps = 64;
        private const int StarCount = 40;
        private const int StarSeed = 1234;
        private const float DistanceFromCamera = 50f;
        private const int SortingOrder = -1000;
        private const float FollowSpeed = 1f;
        private const float MinStarPixels = 2f;
        private const float MaxStarPixels = 3f;
        private const float TwinkleDepth = 0.3f;
        private const float MinTwinkleHz = 0.5f;
        private const float MaxTwinkleHz = 1.5f;

        private struct Star
        {
            public Transform Transform;
            public SpriteRenderer Renderer;
            public Vector2 Position; // -0.5..0.5 of the view
            public float Pixels;
            public float Phase;
            public float Frequency;
        }

        private Camera sceneCamera;
        private Color dayColor;
        private Texture2D gradient;
        private Transform gradientTransform;
        private Star[] stars;
        private readonly Color32[] pixels = new Color32[GradientSteps];
        private float targetHeight;
        private float shownHeight;

        public static SkyBackground Create(Camera camera, Color dayColor)
        {
            var sky = new GameObject("Sky").AddComponent<SkyBackground>();
            sky.sceneCamera = camera;
            sky.dayColor = dayColor;
            sky.transform.SetParent(camera.transform, false);
            sky.Build();
            return sky;
        }

        public void SetTargetHeight(float height) => targetHeight = height;

        // A new round starts in daylight at once.
        public void ResetToDay()
        {
            targetHeight = 0f;
            shownHeight = 0f;
            Refresh();
        }

        private void Build()
        {
            gradient = new Texture2D(1, GradientSteps, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
            };
            var gradientObject = new GameObject("Gradient");
            gradientObject.transform.SetParent(transform, false);
            gradientTransform = gradientObject.transform;
            var gradientRenderer = gradientObject.AddComponent<SpriteRenderer>();
            // Pixels per unit = texture height: the sprite is 1/64 unit wide and one unit tall.
            gradientRenderer.sprite = Sprite.Create(gradient, new Rect(0f, 0f, 1f, GradientSteps), new Vector2(0.5f, 0.5f), GradientSteps);
            gradientRenderer.sortingOrder = SortingOrder;

            var white = new Texture2D(1, 1, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            white.SetPixel(0, 0, Color.white);
            white.Apply();
            Sprite square = Sprite.Create(white, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);

            var random = new System.Random(StarSeed);
            stars = new Star[StarCount];
            for (int i = 0; i < StarCount; i++)
            {
                var starObject = new GameObject("Star");
                starObject.transform.SetParent(transform, false);
                var renderer = starObject.AddComponent<SpriteRenderer>();
                renderer.sprite = square;
                renderer.sortingOrder = SortingOrder + 1;
                stars[i] = new Star
                {
                    Transform = starObject.transform,
                    Renderer = renderer,
                    Position = new Vector2((float)random.NextDouble() - 0.5f, (float)random.NextDouble() - 0.5f),
                    Pixels = random.Next(2) == 0 ? MinStarPixels : MaxStarPixels,
                    Phase = (float)random.NextDouble() * Mathf.PI * 2f,
                    Frequency = Mathf.Lerp(MinTwinkleHz, MaxTwinkleHz, (float)random.NextDouble()),
                };
            }
            Refresh();
        }

        private void LateUpdate()
        {
            // Smooth, so a collapse doesn't make the colors jump.
            shownHeight = Mathf.Lerp(shownHeight, targetHeight, 1f - Mathf.Exp(-FollowSpeed * Time.deltaTime));
            Refresh();
        }

        private void Refresh()
        {
            float viewHeight = 2f * sceneCamera.orthographicSize;
            float viewWidth = viewHeight * sceneCamera.aspect;
            transform.localPosition = new Vector3(0f, 0f, DistanceFromCamera);

            SkyPalette.Evaluate(shownHeight, dayColor, out Color top, out Color bottom);
            for (int i = 0; i < GradientSteps; i++)
                pixels[i] = Color.Lerp(bottom, top, (i + 0.5f) / GradientSteps);
            gradient.SetPixels32(pixels);
            gradient.Apply(false);
            gradientTransform.localScale = new Vector3(viewWidth * GradientSteps, viewHeight, 1f);

            float starAlpha = SkyPalette.StarAlpha(shownHeight);
            float unitsPerPixel = viewHeight / Mathf.Max(1, sceneCamera.pixelHeight);
            for (int i = 0; i < stars.Length; i++)
            {
                Star star = stars[i];
                star.Renderer.enabled = starAlpha > 0f;
                if (starAlpha <= 0f)
                    continue;
                float wave = 0.5f + 0.5f * Mathf.Sin(Time.time * star.Frequency * 2f * Mathf.PI + star.Phase);
                star.Renderer.color = new Color(1f, 1f, 1f, starAlpha * (1f - TwinkleDepth * wave));
                star.Transform.localPosition = new Vector3(star.Position.x * viewWidth, star.Position.y * viewHeight, 0f);
                float side = star.Pixels * unitsPerPixel;
                star.Transform.localScale = new Vector3(side, side, 1f);
            }
        }
    }
}
