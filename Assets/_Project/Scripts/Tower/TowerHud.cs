using UnityEngine;
using UnityEngine.UI;
using ZeroHour.UI;

namespace ZeroHour.Tower
{
    // Stats bar at the top of the screen, round timer and the end screen, built from code.
    public class TowerHud : MonoBehaviour
    {
        private const string WarningColor = "#ff5555";
        private const string NewBestLabel = "NEW BEST!";
        private const string ComboColor = "#ffd84d";
        private const int TimeChangeFontSize = 50;
        private const float TimeChangeRise = 40f;
        private const float TimeChangeDuration = 0.6f;
        private static readonly Color TimeBonusColor = new Color(0.45f, 1f, 0.45f);
        private static readonly Color TimePenaltyColor = new Color(1f, 0.33f, 0.33f);
        private const float TopLayoutBottom = 0.62f;
        private const float TopLayoutPanelAlpha = 0.35f;
        private const float NewBestPulseScale = 1.1f;
        private const float NewBestPulseFrequency = 2f;
        private static readonly Color NewBestColor = new Color(1f, 0.85f, 0.2f);

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
        private RectTransform muteButton;
        private Text muteLabel;
        private Text tutorialText;
        private Text newBestText;
        private Canvas hudCanvas;

        public int TimerWarningSeconds { get; set; } = 10;
        private bool isTopLayout;

        // HUD label of the population stat; the version can rename it.
        public string PopulationLabel { get; set; } = "Pop";
        private Color tutorialOutlineColor;
        private bool isTutorialFading;

        private void Awake()
        {
            var canvasObject = new GameObject("HUD Canvas", typeof(Canvas), typeof(CanvasScaler));
            canvasObject.transform.SetParent(transform, false);

            var canvas = canvasObject.GetComponent<Canvas>();
            hudCanvas = canvas;
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
            Text text = CreateText(name, hudCanvas.transform, size, TextAnchor.MiddleCenter);
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
                tutorialText = CreateText("Tutorial", hudCanvas.transform, tutorialFontSize, TextAnchor.MiddleCenter);
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
            if (newBestText != null && newBestText.gameObject.activeInHierarchy)
            {
                float pulse = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * NewBestPulseFrequency * 2f * Mathf.PI);
                newBestText.rectTransform.localScale = Vector3.one * Mathf.Lerp(1f, NewBestPulseScale, pulse);
            }

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

        // "+1.5s" / "-3s" under the timer. applied is what the clock really changed by (a bonus over the
        // limit is lost); requested decides the color, so a bonus at the limit still reads as a bonus.
        public void ShowTimeChange(float applied, float requested)
        {
            bool isBonus = requested > 0f;
            string label = (isBonus ? "+" : "\u2212") + Mathf.Abs(isBonus ? requested : applied).ToString("0.#", System.Globalization.CultureInfo.InvariantCulture) + "s";
            Text text = CreateText("Time Change", hudCanvas.transform, TimeChangeFontSize, TextAnchor.UpperRight);
            RectTransform rect = text.rectTransform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(1f, 1f);
            rect.sizeDelta = new Vector2(300f, TimeChangeFontSize * 1.5f);
            rect.anchoredPosition = new Vector2(-30f, -30f - timerFontSize * 1.2f);
            text.text = label;
            text.gameObject.AddComponent<FloatingText>().Init(isBonus ? TimeBonusColor : TimePenaltyColor, TimeChangeRise, TimeChangeDuration);
        }

        // Small toggle in the bottom-right corner, drawn above the end screen. TowerGame hit-tests it.
        public void AddMuteButton(bool isMuted)
        {
            var buttonObject = new GameObject("Mute Button", typeof(RectTransform), typeof(Image));
            buttonObject.transform.SetParent(hudCanvas.transform, false);
            muteButton = (RectTransform)buttonObject.transform;
            muteButton.anchorMin = muteButton.anchorMax = muteButton.pivot = new Vector2(1f, 0f);
            muteButton.anchoredPosition = new Vector2(-40f, 40f);
            muteButton.sizeDelta = new Vector2(220f, 120f);
            buttonObject.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.25f);
            buttonObject.GetComponent<Image>().raycastTarget = false;

            muteLabel = CreateText("Label", muteButton, fontSize, TextAnchor.MiddleCenter);
            muteLabel.rectTransform.anchorMin = Vector2.zero;
            muteLabel.rectTransform.anchorMax = Vector2.one;
            muteLabel.rectTransform.sizeDelta = Vector2.zero;
            SetMuted(isMuted);
        }

        public void SetMuted(bool isMuted) => muteLabel.text = isMuted ? "\u266A off" : "\u266A";

        public bool IsOverMuteButton(Vector2 screenPosition) =>
            muteButton != null && muteButton.gameObject.activeInHierarchy &&
            RectTransformUtility.RectangleContainsScreenPoint(muteButton, screenPosition, null);

        public bool IsOverMenuButton(Vector2 screenPosition) =>
            menuButton != null && menuButton.gameObject.activeInHierarchy &&
            RectTransformUtility.RectangleContainsScreenPoint(menuButton, screenPosition, null);

        public void Refresh(CityStats stats, float stability, float timeLeft)
        {
            string stabilityColor = stability > 0.5f ? "white" : stability > 0.25f ? "#ffcc33" : WarningColor;
            statsText.text =
                $"Height {stats.Height}\n" +
                $"$ {stats.Money:0} (+{stats.Income:0.0}/s)" +
                (stats.ComboMultiplier > 1 ? $" <color={ComboColor}>COMBO x{stats.ComboMultiplier}</color>" : "") + "\n" +
                $"{Label(PopulationLabel, 0)} {stats.Population}\n" +
                $"{Label("Jobs", 1)} {stats.Jobs}\n" +
                $"{Label("Energy", 2)} {Signed(stats.Energy)}\n" +
                $"{Label("Joy", 3)} {Signed(stats.Happiness)}\n" +
                $"<color={stabilityColor}>Stability {stability * 100f:0}%</color>" +
                (stats.PowerWarning == null ? "" : $"\n<color={WarningColor}>{stats.PowerWarning}</color>");

            int seconds = Mathf.CeilToInt(Mathf.Max(0f, timeLeft));
            timerText.text = seconds <= TimerWarningSeconds ? $"<color={WarningColor}>{seconds}s</color>" : $"{seconds}s";
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

        // End screen text in the top third over a lighter panel, leaving the rest of the screen to the tower.
        public void UseTopLayout()
        {
            isTopLayout = true;
            gameOverPanel.GetComponent<Image>().color = new Color(0f, 0f, 0f, TopLayoutPanelAlpha);
            var resultRect = gameOverText.rectTransform;
            resultRect.anchorMin = new Vector2(0f, TopLayoutBottom);
            resultRect.anchorMax = Vector2.one;
            resultRect.offsetMin = new Vector2(30f, 0f);
            resultRect.offsetMax = new Vector2(-30f, -40f);
            gameOverText.alignment = TextAnchor.UpperCenter;
        }

        // bestScore is null when the version doesn't track records.
        public void ShowGameOver(int height, float money, int score, int? bestScore = null, bool isNewBest = false)
        {
            string text = isTopLayout
                ? "TIME'S UP!\n" +
                  $"Height  {height}    Money  $ {money:0}\n" +
                  $"<size={gameOverFontSize + 20}>Score  {score}</size>"
                : "TIME'S UP!\n\n" +
                  $"Height  {height}\n" +
                  $"Money  $ {money:0}\n\n" +
                  $"<size={gameOverFontSize + 20}>Score  {score}</size>";
            int bestLineIndex = -1;
            if (bestScore.HasValue)
            {
                text += "\n";
                bestLineIndex = text.Split('\n').Length - 1;
                // A new record gets its own pulsing text; an invisible copy keeps the line's space.
                text += isNewBest ? $"<color=#00000000>{NewBestLabel}</color>" : $"Best  {bestScore.Value}";
            }
            text += (isTopLayout ? "\n" : "\n\n") + $"<size={gameOverFontSize * 3 / 4}>Tap to play again</size>";
            gameOverText.text = text;
            gameOverPanel.SetActive(true);
            // The summary repeats the stats; the panel and the timer would sit under the top-layout text.
            if (isTopLayout)
                SetGameplayHudVisible(false);

            if (newBestText != null)
                newBestText.gameObject.SetActive(false);
            if (isNewBest)
                ShowNewBest(bestLineIndex);
        }

        private void ShowNewBest(int lineIndex)
        {
            if (newBestText == null)
            {
                newBestText = CreateText("New Best", gameOverText.transform, gameOverFontSize, TextAnchor.MiddleCenter);
                newBestText.text = NewBestLabel;
                newBestText.color = NewBestColor;
                newBestText.rectTransform.sizeDelta = new Vector2(800f, gameOverFontSize * 1.5f);
            }

            // Place it on the reserved line of the result text.
            Canvas.ForceUpdateCanvases();
            TextGenerator generator = gameOverText.cachedTextGenerator;
            float y = 0f;
            if (lineIndex >= 0 && lineIndex < generator.lineCount)
            {
                UILineInfo line = generator.lines[lineIndex];
                y = (line.topY - line.height / 2f) / gameOverText.pixelsPerUnit;
            }
            newBestText.rectTransform.anchoredPosition = new Vector2(0f, y);
            newBestText.gameObject.SetActive(true);
        }

        public void HideGameOver()
        {
            gameOverPanel.SetActive(false);
            SetGameplayHudVisible(true);
        }

        private void SetGameplayHudVisible(bool visible)
        {
            statsText.transform.parent.gameObject.SetActive(visible);
            timerText.gameObject.SetActive(visible);
        }
    }
}
