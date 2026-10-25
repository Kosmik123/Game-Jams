using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ZeroHour.Tower;
using ZeroHour.UI;

namespace ZeroHour.Menu
{
    // First screen: picks the jam or post-jam version, then loads the game.
    public class VersionSelectMenu : MonoBehaviour
    {
        [SerializeField]
        private int titleFontSize = 130;

        [SerializeField]
        private int buttonFontSize = 76;

        [SerializeField]
        private Vector2 buttonSize = new Vector2(760f, 220f);

        [SerializeField]
        private int footerFontSize = 40;

        private void Awake()
        {
            Camera camera = Camera.main;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = TowerGame.SkyColor;

            UiFactory.EnsureEventSystem();
            Canvas canvas = UiFactory.CreateCanvas("Menu Canvas", transform, 50);

            Text title = UiFactory.CreateText("Title", canvas.transform, titleFontSize, TextAnchor.MiddleCenter);
            title.text = "Stack City";
            Place(title.rectTransform, 520f, new Vector2(1000f, 220f));

            Button jamButton = UiFactory.CreateButton("Jam version", canvas.transform, buttonFontSize, buttonSize);
            Place((RectTransform)jamButton.transform, 80f, buttonSize);
            jamButton.onClick.AddListener(() => Play(GameVersionType.Jam));

            Button postJamButton = UiFactory.CreateButton("Post-jam version", canvas.transform, buttonFontSize, buttonSize);
            Place((RectTransform)postJamButton.transform, -200f, buttonSize);
            postJamButton.onClick.AddListener(() => Play(GameVersionType.PostJam));

            Text footer = UiFactory.CreateText("Footer", canvas.transform, footerFontSize, TextAnchor.MiddleCenter);
            footer.text = "0h Game Jam 2026";
            Place(footer.rectTransform, -420f, new Vector2(1000f, 80f));
        }

        // Centered horizontally, y relative to the screen center.
        private static void Place(RectTransform rect, float y, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, y);
            rect.sizeDelta = size;
        }

        private static void Play(GameVersionType version)
        {
            GameVersion.Selected = version;
            SceneManager.LoadScene(GameBootstrap.GameScene);
        }
    }
}
