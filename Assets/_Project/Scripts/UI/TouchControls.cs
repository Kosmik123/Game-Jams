using Bipolar.TouchSticks;
using UnityEngine;

namespace PayWithPixels
{
    /// <summary>
    /// Dwa plywajace joysticki na ekranie poziomym: lewa polowa ekranu = ruch, prawa = celowanie.
    /// Trzymanie prawego joysticka strzela. Wlaczone na platformie mobilnej albo gdy adres strony ma "touch=1"
    /// (wtedy na komputerze mysz robi za jeden palec). Dash i freeze na razie pominiete.
    /// Sama obsluga dotyku jest w module Bipolar.TouchSticks; ta klasa tylko laczy go z gra.
    /// </summary>
    [DefaultExecutionOrder(-200)]
    public class TouchControls : MonoBehaviour
    {
        private const float MoveDeadZone = 0.12f;
        private const float AimDeadZone = 0.2f;

        private VirtualJoystick moveStick;
        private VirtualJoystick aimStick;
        private GUIStyle noticeStyle;

        public static bool Enabled => TouchPointers.Enabled;
        public static Vector2 Move { get; private set; }
        public static Vector2 Aim { get; private set; }     // kierunek celowania; zero = zostaw poprzedni kat
        public static bool Firing { get; private set; }

        private void Awake()
        {
            if (Enabled == false)
            {
                enabled = false;
                return;
            }

        }

        private void Start()
        {
            moveStick = gameObject.AddComponent<VirtualJoystick>();
            moveStick.Configure(new Rect(0f, 0f, 0.5f, 1f), new Vector2(0f, 0f), MoveDeadZone, false);
            aimStick = gameObject.AddComponent<VirtualJoystick>();
            aimStick.Configure(new Rect(0.5f, 0f, 0.5f, 1f), new Vector2(1f, 0f), AimDeadZone, true);
        }

        private static bool GameplayActive()
        {
            var game = Game.Instance;
            return game != null && !game.Paused && (game.state == Game.State.Playing || game.state == Game.State.Clearing);
        }

        private void Update()
        {
            bool active = GameplayActive();
            var foreground = Palette.Foreground;   // kolor z aktualnej palety strefy (tez po odwroceniu)
            moveStick.InputEnabled = active;
            aimStick.InputEnabled = active;
            moveStick.Color = foreground;
            aimStick.Color = foreground;

            Move = moveStick.Value;
            Aim = aimStick.Value;
            Firing = aimStick.IsPressed;
        }

        private void OnGUI()
        {
            if (Screen.height <= Screen.width)
                return;

            if (noticeStyle == null)
                noticeStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = Mathf.Max(18, Screen.width / 18), fontStyle = FontStyle.Bold };

            GUI.color = Palette.Foreground;
            GUI.Label(new Rect(0, Screen.height * 0.4f, Screen.width, Screen.height * 0.2f), "ROTATE YOUR DEVICE", noticeStyle);
        }
    }
}
