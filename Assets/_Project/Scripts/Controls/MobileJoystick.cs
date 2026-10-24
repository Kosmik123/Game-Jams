using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace ZeroHour.Controls
{
    // Builds the on-screen joystick from code, so no scene setup is needed.
    // Visible on mobile browsers, or after the first touch (e.g. iPad reporting itself as a Mac).
    public class MobileJoystick : MonoBehaviour
    {
        [SerializeField]
        private Vector2 referenceResolution = new Vector2(1080f, 1920f);

        [SerializeField]
        private float backgroundSize = 300f;

        [SerializeField]
        private float knobSize = 130f;

        [SerializeField]
        private Vector2 margin = new Vector2(120f, 120f);

        private GameObject canvasObject;

        public bool IsVisible => canvasObject.activeSelf;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreateInstance()
        {
            var joystickObject = new GameObject(nameof(MobileJoystick));
            DontDestroyOnLoad(joystickObject);
            joystickObject.AddComponent<MobileJoystick>();
        }

        private void Awake()
        {
            EnsureEventSystem();
            canvasObject = BuildJoystick();
            canvasObject.SetActive(Application.isMobilePlatform);
        }

        private void Update()
        {
            if (IsVisible)
                return;

            var touchscreen = Touchscreen.current;
            if (touchscreen != null && touchscreen.primaryTouch.press.wasPressedThisFrame)
                Show();
        }

        public void Show() => canvasObject.SetActive(true);

        public void Hide() => canvasObject.SetActive(false);

        private void EnsureEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() != null)
                return;

            var eventSystemObject = new GameObject(nameof(EventSystem), typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventSystemObject.transform.SetParent(transform, false);
        }

        private GameObject BuildJoystick()
        {
            var canvasGameObject = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGameObject.transform.SetParent(transform, false);

            var canvas = canvasGameObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = canvasGameObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = referenceResolution;
            scaler.matchWidthOrHeight = 0.5f;

            Sprite circle = CreateCircleSprite(128);

            Image background = CreateImage("Background", canvasGameObject.transform, circle, backgroundSize, new Color(1f, 1f, 1f, 0.25f));
            background.raycastTarget = false;
            RectTransform backgroundTransform = background.rectTransform;
            backgroundTransform.anchorMin = backgroundTransform.anchorMax = backgroundTransform.pivot = Vector2.zero;
            backgroundTransform.anchoredPosition = margin;

            Image knob = CreateImage("Knob", backgroundTransform, circle, knobSize, new Color(1f, 1f, 1f, 0.6f));
            // Let the whole background area grab the stick, not only the small knob.
            float range = (backgroundSize - knobSize) / 2f;
            knob.raycastPadding = new Vector4(-range, -range, -range, -range);

            // Configure the stick while inactive, so it binds to the right control on enable.
            knob.gameObject.SetActive(false);
            var stick = knob.gameObject.AddComponent<OnScreenStick>();
            stick.controlPath = "<Gamepad>/leftStick";
            stick.movementRange = range;
            knob.gameObject.SetActive(true);

            return canvasGameObject;
        }

        private static Image CreateImage(string name, Transform parent, Sprite sprite, float size, Color color)
        {
            var imageObject = new GameObject(name, typeof(RectTransform), typeof(Image));
            imageObject.transform.SetParent(parent, false);

            var image = imageObject.GetComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.rectTransform.sizeDelta = new Vector2(size, size);
            return image;
        }

        private static Sprite CreateCircleSprite(int resolution)
        {
            var texture = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
            float radius = resolution / 2f;
            var pixels = new Color32[resolution * resolution];
            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(radius, radius));
                    byte alpha = (byte)(Mathf.Clamp01(radius - distance) * 255f);
                    pixels[y * resolution + x] = new Color32(255, 255, 255, alpha);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, resolution, resolution), new Vector2(0.5f, 0.5f));
        }
    }
}
