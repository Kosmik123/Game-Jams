using UnityEngine;
using UnityEngine.UI;

namespace ZeroHour.Tower
{
    // Stats bar at the top of the screen, round timer and the end screen, built from code.
    public class TowerHud : MonoBehaviour
    {
        private const string WarningColor = "#ff5555";

        [SerializeField]
        private Vector2 referenceResolution = new Vector2(1080f, 1920f);

        [SerializeField]
        private int fontSize = 60;

        [SerializeField]
        private int timerFontSize = 72;

        [SerializeField]
        private int gameOverFontSize = 80;

        private Text statsText;
        private Text timerText;
        private GameObject gameOverPanel;
        private Text gameOverText;

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

            // Panel in the top-left corner that shrinks to fit the text, so it hides as little as possible.
            var statsPanel = new GameObject("Stats Panel", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            statsPanel.transform.SetParent(canvasObject.transform, false);
            var panelRect = (RectTransform)statsPanel.transform;
            panelRect.anchorMin = new Vector2(0f, 1f);
            panelRect.anchorMax = new Vector2(0f, 1f);
            panelRect.pivot = new Vector2(0f, 1f);
            panelRect.anchoredPosition = new Vector2(20f, -20f);
            statsPanel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.3f);
            statsPanel.GetComponent<Image>().raycastTarget = false;
            var layout = statsPanel.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(20, 20, 12, 12);
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            var fitter = statsPanel.GetComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            statsText = CreateText("Stats", statsPanel.transform, fontSize, TextAnchor.UpperLeft);

            timerText = CreateText("Timer", canvasObject.transform, timerFontSize, TextAnchor.UpperRight);
            var timerRect = timerText.rectTransform;
            timerRect.anchorMin = new Vector2(1f, 1f);
            timerRect.anchorMax = new Vector2(1f, 1f);
            timerRect.pivot = new Vector2(1f, 1f);
            timerRect.anchoredPosition = new Vector2(-30f, -30f);
            timerRect.sizeDelta = new Vector2(300f, 100f);

            gameOverPanel = new GameObject("Game Over", typeof(RectTransform), typeof(Image));
            gameOverPanel.transform.SetParent(canvasObject.transform, false);
            var gameOverRect = (RectTransform)gameOverPanel.transform;
            gameOverRect.anchorMin = Vector2.zero;
            gameOverRect.anchorMax = Vector2.one;
            gameOverRect.sizeDelta = Vector2.zero;
            gameOverPanel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.65f);

            gameOverText = CreateText("Result", gameOverPanel.transform, gameOverFontSize, TextAnchor.MiddleCenter);
            var resultRect = gameOverText.rectTransform;
            resultRect.anchorMin = Vector2.zero;
            resultRect.anchorMax = Vector2.one;
            resultRect.sizeDelta = new Vector2(-60f, 0f);
            gameOverPanel.SetActive(false);
        }

        private static Text CreateText(string name, Transform parent, int size, TextAnchor alignment)
        {
            var textObject = new GameObject(name, typeof(RectTransform), typeof(Text), typeof(Outline));
            textObject.transform.SetParent(parent, false);
            textObject.GetComponent<Outline>().effectDistance = new Vector2(3f, -3f);

            var text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.alignment = alignment;
            text.color = Color.white;
            text.raycastTarget = false;
            text.supportRichText = true;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            return text;
        }

        public void Refresh(CityStats stats, float stability, float timeLeft)
        {
            string stabilityColor = stability > 0.5f ? "white" : stability > 0.25f ? "#ffcc33" : WarningColor;
            statsText.text =
                $"Height {stats.Height}\n" +
                $"$ {stats.Money:0} (+{stats.Income:0.0}/s)\n" +
                $"{Label("Pop", 0)} {stats.Population}\n" +
                $"{Label("Jobs", 1)} {stats.Jobs}\n" +
                $"{Label("Energy", 2)} {Signed(stats.Energy)}\n" +
                $"{Label("Joy", 3)} {Signed(stats.Happiness)}\n" +
                $"<color={stabilityColor}>Stability {stability * 100f:0}%</color>" +
                (stats.IsPowered ? "" : $"\n<color={WarningColor}>BLACKOUT!</color>");

            int seconds = Mathf.CeilToInt(Mathf.Max(0f, timeLeft));
            timerText.text = seconds <= 10 ? $"<color={WarningColor}>{seconds}s</color>" : $"{seconds}s";
        }

        // Stat label in the color of the floor type that produces it (order of FloorType.All).
        private static string Label(string label, int floorTypeIndex)
        {
            string color = ColorUtility.ToHtmlStringRGB(FloorType.All[floorTypeIndex].Color);
            return $"<color=#{color}>{label}</color>";
        }

        private static string Signed(int value)
        {
            string text = value.ToString("+0;-0;0");
            return value < 0 ? $"<color={WarningColor}>{text}</color>" : text;
        }

        public void ShowGameOver(int height, float money, int score)
        {
            gameOverText.text =
                "TIME'S UP!\n\n" +
                $"Height  {height}\n" +
                $"Money  $ {money:0}\n\n" +
                $"<size={gameOverFontSize + 20}>Score  {score}</size>\n\n" +
                $"<size={gameOverFontSize * 3 / 4}>Tap to play again</size>";
            gameOverPanel.SetActive(true);
        }

        public void HideGameOver() => gameOverPanel.SetActive(false);
    }
}
