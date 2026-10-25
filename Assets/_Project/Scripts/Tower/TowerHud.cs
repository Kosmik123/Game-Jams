using UnityEngine;
using UnityEngine.UI;
using ZeroHour.UI;

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

        [SerializeField]
        private int tutorialFontSize = 64;

        [SerializeField]
        private float tutorialFadeDuration = 0.5f;

        private Text statsText;
        private Text timerText;
        private GameObject gameOverPanel;
        private Text gameOverText;
        private RectTransform menuButton;
        private Text tutorialText;
        private Color tutorialOutlineColor;
        private bool isTutorialFading;

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

        // Text that follows a world position (positioned by the caller), drawn below the HUD panels.
        public Text CreateWorldText(string name, int size)
        {
            Text text = CreateText(name, statsText.canvas.transform, size, TextAnchor.MiddleCenter);
            text.rectTransform.SetAsFirstSibling();
            text.rectTransform.sizeDelta = new Vector2(600f, size * 1.5f);
            return text;
        }

        private static Text CreateText(string name, Transform parent, int size, TextAnchor alignment) =>
            UiFactory.CreateText(name, parent, size, alignment);

        // Small "Menu" box in the bottom-left corner of the end screen. TowerGame checks taps
        // against it itself, because the restart reacts to the press, before a Button would.
        public void AddMenuButton()
        {
            var buttonObject = new GameObject("Menu Button", typeof(RectTransform), typeof(Image));
            buttonObject.transform.SetParent(gameOverPanel.transform, false);
            menuButton = (RectTransform)buttonObject.transform;
            menuButton.anchorMin = Vector2.zero;
            menuButton.anchorMax = Vector2.zero;
            menuButton.pivot = Vector2.zero;
            menuButton.anchoredPosition = new Vector2(40f, 40f);
            menuButton.sizeDelta = new Vector2(300f, 140f);
            buttonObject.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.2f);
            buttonObject.GetComponent<Image>().raycastTarget = false;

            Text label = CreateText("Label", menuButton, gameOverFontSize * 3 / 4, TextAnchor.MiddleCenter);
            label.text = "Menu";
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.sizeDelta = Vector2.zero;
        }

        public void ShowTutorial(string message)
        {
            if (tutorialText == null)
            {
                tutorialText = CreateText("Tutorial", statsText.canvas.transform, tutorialFontSize, TextAnchor.MiddleCenter);
                tutorialText.rectTransform.SetSiblingIndex(gameOverPanel.transform.GetSiblingIndex());
                tutorialText.rectTransform.sizeDelta = new Vector2(1000f, tutorialFontSize * 1.5f);
                tutorialOutlineColor = tutorialText.GetComponent<Outline>().effectColor;
            }
            tutorialText.text = message;
            isTutorialFading = false;
            SetTutorialAlpha(1f);
            tutorialText.gameObject.SetActive(true);
        }

        public void FadeOutTutorial()
        {
            if (tutorialText != null && tutorialText.gameObject.activeSelf)
                isTutorialFading = true;
        }

        public void HideTutorial()
        {
            if (tutorialText != null)
                tutorialText.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (!isTutorialFading)
                return;
            float alpha = tutorialText.color.a - Time.deltaTime / tutorialFadeDuration;
            if (alpha <= 0f)
            {
                isTutorialFading = false;
                HideTutorial();
                return;
            }
            SetTutorialAlpha(alpha);
        }

        private void SetTutorialAlpha(float alpha)
        {
            tutorialText.color = new Color(1f, 1f, 1f, alpha);
            Color outline = tutorialOutlineColor;
            outline.a *= alpha;
            tutorialText.GetComponent<Outline>().effectColor = outline;
        }

        public bool IsOverMenuButton(Vector2 screenPosition) =>
            menuButton != null && menuButton.gameObject.activeInHierarchy &&
            RectTransformUtility.RectangleContainsScreenPoint(menuButton, screenPosition, null);

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
                (stats.PowerWarning == null ? "" : $"\n<color={WarningColor}>{stats.PowerWarning}</color>");

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
