using UnityEngine;
using UnityEngine.UI;

namespace ZeroHour.Tower
{
    // Stats bar at the top of the screen, built from code.
    public class TowerHud : MonoBehaviour
    {
        [SerializeField]
        private Vector2 referenceResolution = new Vector2(1080f, 1920f);

        [SerializeField]
        private int fontSize = 46;

        private Text statsText;

        private void Awake()
        {
            var canvasObject = new GameObject("HUD Canvas", typeof(Canvas), typeof(CanvasScaler));
            canvasObject.transform.SetParent(transform, false);

            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = referenceResolution;
            scaler.matchWidthOrHeight = 0.5f;

            var textObject = new GameObject("Stats", typeof(RectTransform), typeof(Text), typeof(Outline));
            textObject.transform.SetParent(canvasObject.transform, false);

            var rectTransform = (RectTransform)textObject.transform;
            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(1f, 1f);
            rectTransform.pivot = new Vector2(0.5f, 1f);
            rectTransform.anchoredPosition = new Vector2(0f, -30f);
            rectTransform.sizeDelta = new Vector2(-60f, 300f);

            statsText = textObject.GetComponent<Text>();
            statsText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            statsText.fontSize = fontSize;
            statsText.alignment = TextAnchor.UpperCenter;
            statsText.color = Color.white;
            statsText.raycastTarget = false;
            statsText.supportRichText = true;

            textObject.GetComponent<Outline>().effectDistance = new Vector2(3f, -3f);
        }

        public void Refresh(CityStats stats, float stability)
        {
            string energyColor = stats.IsPowered ? "white" : "#ff5555";
            string happinessColor = stats.Happiness >= 0 ? "white" : "#ff5555";
            string stabilityColor = stability > 0.5f ? "white" : stability > 0.25f ? "#ffcc33" : "#ff5555";
            statsText.text =
                $"Height {stats.Height}   $ {stats.Money:0} (+{stats.Income:0.0}/s)\n" +
                $"Pop {stats.Population}/{stats.Jobs} jobs   " +
                $"<color={energyColor}>Energy {stats.Energy:+0;-0;0}</color>   " +
                $"<color={happinessColor}>Joy {stats.Happiness:+0;-0;0}</color>\n" +
                $"<color={stabilityColor}>Stability {stability * 100f:0}%</color>" +
                (stats.IsPowered ? "" : "   <color=#ff5555>BLACKOUT!</color>");
        }
    }
}
