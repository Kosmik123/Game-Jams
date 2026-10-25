using UnityEngine;
using UnityEngine.UI;

namespace ZeroHour.Tower
{
    // Stats bar at the top of the screen, round timer and the end screen, built from code.
    public class TowerHud : MonoBehaviour
    {
        [SerializeField]
        private Vector2 referenceResolution = new Vector2(1080f, 1920f);

        [SerializeField]
        private int fontSize = 46;

        [SerializeField]
        private int timerFontSize = 60;

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

            statsText = CreateText("Stats", canvasObject.transform, fontSize, TextAnchor.UpperCenter);
            var statsRect = statsText.rectTransform;
            statsRect.anchorMin = new Vector2(0f, 1f);
            statsRect.anchorMax = new Vector2(1f, 1f);
            statsRect.pivot = new Vector2(0.5f, 1f);
            statsRect.anchoredPosition = new Vector2(0f, -30f);
            statsRect.sizeDelta = new Vector2(-60f, 300f);

            timerText = CreateText("Timer", canvasObject.transform, timerFontSize, TextAnchor.UpperRight);
            var timerRect = timerText.rectTransform;
            timerRect.anchorMin = new Vector2(1f, 1f);
            timerRect.anchorMax = new Vector2(1f, 1f);
            timerRect.pivot = new Vector2(1f, 1f);
            timerRect.anchoredPosition = new Vector2(-30f, -30f);
            timerRect.sizeDelta = new Vector2(300f, 100f);

            gameOverPanel = new GameObject("Game Over", typeof(RectTransform), typeof(Image));
            gameOverPanel.transform.SetParent(canvasObject.transform, false);
            var panelRect = (RectTransform)gameOverPanel.transform;
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.sizeDelta = Vector2.zero;
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

            int seconds = Mathf.CeilToInt(Mathf.Max(0f, timeLeft));
            timerText.text = seconds <= 10 ? $"<color=#ff5555>{seconds}s</color>" : $"{seconds}s";
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
