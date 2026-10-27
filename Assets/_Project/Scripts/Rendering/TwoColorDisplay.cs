using UnityEngine;
using UnityEngine.UI;

namespace PayWithPixels
{
    /// <summary>
    /// Kamera gry renderuje do malej RenderTexture (Point, bez AA), ktora jest wyswietlana
    /// przez material "Jam/TwoColor" (prog luminancji -> 2 kolory, mgla). Pikselizacja = rozmiar RT.
    /// </summary>
    public class TwoColorDisplay : MonoBehaviour
    {
        public const int BaseHeight = 180;
        private const float Aspect = 16f / 9f;
        private const float MinHeight = 30;

        private Camera worldCamera;
        private RenderTexture rt;
        private RawImage image;
        private Image backdrop;
        private Material material;
        private int pixelLevel = -1;

        private Vector2 fogWorldCenter;
        private float fogWorldRadius;

        /// <summary>Prostokat obrazu gry na ekranie (px), z letterboxem.</summary>
        public Rect ScreenRect { get; private set; }

        public static TwoColorDisplay Create(Transform parent, Camera cam)
        {
            var go = new GameObject("Display");
            go.transform.SetParent(parent, false);
            var d = go.AddComponent<TwoColorDisplay>();
            d.Init(cam);
            return d;
        }

        private void Init(Camera cam)
        {
            worldCamera = cam;

            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;

            backdrop = new GameObject("Backdrop", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
            backdrop.rectTransform.SetParent(transform, false);
            backdrop.raycastTarget = false;
            var br = backdrop.rectTransform;
            br.anchorMin = Vector2.zero; br.anchorMax = Vector2.one; br.offsetMin = br.offsetMax = Vector2.zero;

            image = new GameObject("Game View", typeof(RectTransform), typeof(RawImage)).GetComponent<RawImage>();
            image.rectTransform.SetParent(transform, false);
            image.raycastTarget = false;

            material = new Material(Resources.Load<Shader>("TwoColor"));
            image.material = material;
            material.SetFloat("_Aspect", Aspect);

            SetPixelLevel(0);
            Palette.Changed += ApplyColors;
            ApplyColors();
        }

        private void OnDestroy()
        {
            Palette.Changed -= ApplyColors;
            if (rt != null) rt.Release();
        }

        private void ApplyColors()
        {
            material.SetColor("_Bg", Palette.Background);
            material.SetColor("_Fg", Palette.Foreground);
            backdrop.color = Palette.Background;
        }

        /// <summary>Cena "Pikselizacja": 0 = pelna rozdzielczosc (320x180), kazdy poziom ja zmniejsza.</summary>
        public void SetPixelLevel(int level)
        {
            if (level == pixelLevel) return;
            pixelLevel = level;
            float h = Mathf.Max(MinHeight, BaseHeight / (1f + 0.5f * level));
            int height = Mathf.RoundToInt(h);
            int width = Mathf.RoundToInt(h * Aspect);

            if (rt != null)
            {
                worldCamera.targetTexture = null;
                rt.Release();
            }
            rt = new RenderTexture(width, height, 16, RenderTextureFormat.ARGB32)
            {
                filterMode = FilterMode.Point,
                antiAliasing = 1,
                useMipMap = false,
                name = "Game RT"
            };
            worldCamera.targetTexture = rt;
            image.texture = rt;
        }

        /// <summary>Cena "Mgla": promien widocznosci w jednostkach swiata, 0 = wylaczona.</summary>
        public void SetFog(float worldRadius) => fogWorldRadius = worldRadius;
        public void SetFogCenter(Vector2 worldPos) => fogWorldCenter = worldPos;

        private void LateUpdate()
        {
            float scale = Mathf.Min(Screen.width / 16f, Screen.height / 9f);
            var size = new Vector2(16f * scale, 9f * scale);
            var rect = image.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = size;
            ScreenRect = new Rect((Screen.width - size.x) * 0.5f, (Screen.height - size.y) * 0.5f, size.x, size.y);

            Vector3 vp = worldCamera.WorldToViewportPoint(fogWorldCenter);
            material.SetVector("_FogCenter", new Vector4(vp.x, vp.y, 0, 0));
            material.SetFloat("_FogRadius", fogWorldRadius > 0 ? fogWorldRadius / (worldCamera.orthographicSize * 2f) : 0f);
        }

        public Vector2 ScreenToWorld(Vector2 screenPos)
        {
            var r = ScreenRect;
            var vp = new Vector3((screenPos.x - r.x) / r.width, (screenPos.y - r.y) / r.height, 0);
            return worldCamera.ViewportToWorldPoint(vp);
        }
    }
}
