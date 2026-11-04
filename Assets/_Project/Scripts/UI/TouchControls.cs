using System.Collections.Generic;
using UnityEngine;

namespace PayWithPixels
{
    /// <summary>
    /// Dwa plywajace joysticki na ekranie poziomym: lewa polowa ekranu = ruch, prawa = celowanie.
    /// Trzymanie prawego joysticka strzela. Wlaczone na platformie mobilnej albo gdy adres strony ma "touch=1"
    /// (wtedy na komputerze mysz robi za jeden palec). Dash i freeze na razie pominiete.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class TouchControls : MonoBehaviour
    {
        public static bool Enabled { get; private set; }
        public static Vector2 Move { get; private set; }
        public static Vector2 Aim { get; private set; }     // kierunek celowania; zero = zostaw poprzedni kat
        public static bool Firing { get; private set; }

        private const int None = int.MinValue;
        private const int MouseId = -1;
        private const float MoveDeadZone = 0.12f;
        private const float AimDeadZone = 0.2f;

        private class Stick
        {
            public int id = None;
            public Vector2 origin, current;
            public bool Held => id != None;
        }

        private struct Pointer
        {
            public int id;
            public Vector2 pos;
            public bool began, ended;
        }

        private readonly Stick moveStick = new Stick();
        private readonly Stick aimStick = new Stick();
        private readonly List<Pointer> pointers = new List<Pointer>();
        private Texture2D ring, disc;
        private GUIStyle noticeStyle;

        private static float Radius => Mathf.Max(60f, Screen.height * 0.13f);

        private void Awake()
        {
            Enabled = Application.isMobilePlatform || Application.absoluteURL.IndexOf("touch=1", System.StringComparison.OrdinalIgnoreCase) >= 0;
            if (Enabled == false) 
            { 
                enabled = false;
                return; 
            }
            ring = MakeTexture(128, 6f);
            disc = MakeTexture(128, 0f);
        }

        private static bool GameplayActive()
        {
            var game = Game.Instance;
            return game != null && !game.Paused && (game.state == Game.State.Playing || game.state == Game.State.Clearing);
        }

        private void Update()
        {
            bool active = GameplayActive();
            Gather();
            UpdateStick(moveStick, true, aimStick, active);
            UpdateStick(aimStick, false, moveStick, active);

            float radius = Radius;
            var m = Vec(moveStick, radius);
            Move = m.magnitude < MoveDeadZone ? Vector2.zero : m;
            var a = Vec(aimStick, radius);
            Aim = a.magnitude < AimDeadZone ? Vector2.zero : a.normalized;
            Firing = aimStick.Held;
        }

        private void Gather()
        {
            pointers.Clear();
            for (int i = 0; i < Input.touchCount; i++)
            {
                var t = Input.GetTouch(i);
                pointers.Add(new Pointer
                {
                    id = t.fingerId,
                    pos = t.position,
                    began = t.phase == TouchPhase.Began,
                    ended = t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled,
                });
            }
            if (!Input.touchSupported && (Input.GetMouseButton(0) || Input.GetMouseButtonUp(0)))
                pointers.Add(new Pointer { id = MouseId, pos = Input.mousePosition, began = Input.GetMouseButtonDown(0), ended = Input.GetMouseButtonUp(0) });
        }

        private void UpdateStick(Stick s, bool leftHalf, Stick other, bool active)
        {
            if (s.Held)
            {
                bool found = false;
                foreach (var p in pointers)
                {
                    if (p.id != s.id) continue;
                    found = true;
                    if (p.ended) s.id = None;
                    else s.current = p.pos;
                    break;
                }
                if (!found || !active) s.id = None;
            }
            if (s.Held || !active) return;
            foreach (var p in pointers)
            {
                if (!p.began || p.ended || p.id == other.id) continue;
                if ((p.pos.x < Screen.width * 0.5f) != leftHalf) continue;
                s.id = p.id;
                s.origin = s.current = p.pos;
                break;
            }
        }

        /// <summary>Wychylenie 0..1; gdy palec wyjdzie poza promien, podstawa joysticka podaza za nim.</summary>
        private static Vector2 Vec(Stick s, float radius)
        {
            if (!s.Held) return Vector2.zero;
            var v = (s.current - s.origin) / radius;
            if (v.magnitude <= 1f) return v;
            s.origin = s.current - v.normalized * radius;
            return v.normalized;
        }

        private void OnGUI()
        {
            if (!Enabled) return;
            if (Screen.height > Screen.width)
            {
                if (noticeStyle == null)
                    noticeStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = Mathf.Max(18, Screen.width / 18), fontStyle = FontStyle.Bold };
                GUI.color = Palette.Foreground;
                GUI.Label(new Rect(0, Screen.height * 0.4f, Screen.width, Screen.height * 0.2f), "ROTATE YOUR DEVICE", noticeStyle);
                return;
            }
            if (!GameplayActive()) return;
            float radius = Radius;
            // stala odleglosc od krawedzi ekranu (nie od areny), wiec na szerokich ekranach joysticki leza w bocznych marginesach
            float inset = Mathf.Max(20f, Screen.height * 0.05f) + radius;
            Draw(moveStick, new Vector2(inset, inset), radius);
            Draw(aimStick, new Vector2(Screen.width - inset, inset), radius);
        }

        private void Draw(Stick s, Vector2 idleCenter, float radius)
        {
            Vector2 center = s.Held ? s.origin : idleCenter;
            Vector2 knob = s.Held ? s.origin + Vector2.ClampMagnitude(s.current - s.origin, radius) : idleCenter;
            var fg = Palette.Foreground;   // kolor z aktualnej palety strefy (tez po odwroceniu)
            GUI.color = new Color(fg.r, fg.g, fg.b, s.Held ? 0.85f : 0.35f);
            GUI.DrawTexture(Square(center, radius * 2f), ring);
            GUI.color = new Color(fg.r, fg.g, fg.b, s.Held ? 0.9f : 0.3f);
            GUI.DrawTexture(Square(knob, radius * 0.8f), disc);
            GUI.color = Color.white;
        }

        private static Rect Square(Vector2 screenCenter, float size) =>
            new Rect(screenCenter.x - size * 0.5f, Screen.height - screenCenter.y - size * 0.5f, size, size);

        /// <summary>Kolo (thickness 0) albo pierscien o danej grubosci w pikselach, z wygladzonym brzegiem.</summary>
        private static Texture2D MakeTexture(int size, float thickness)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear, hideFlags = HideFlags.HideAndDontSave };
            float r = size * 0.5f - 1f;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(size * 0.5f, size * 0.5f));
                    float a = Mathf.Clamp01(r - d);
                    if (thickness > 0f) a = Mathf.Min(a, Mathf.Clamp01(d - (r - thickness)));
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                }
            tex.Apply();
            return tex;
        }
    }
}
