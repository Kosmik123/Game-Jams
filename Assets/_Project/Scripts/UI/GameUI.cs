using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PayWithPixels
{
    /// <summary>HUD, sklep i ekran smierci budowane w kodzie. Tylko 2 kolory z Palette (bez przejsc przyciskow).</summary>
    public class GameUI : MonoBehaviour
    {
        private readonly List<KeyValuePair<Graphic, bool>> tinted = new List<KeyValuePair<Graphic, bool>>(); // true = foreground
        private Font font;
        private Text hud;
        private GameObject shopPanel, deathPanel;
        private Text deathText;
        private RectTransform shopCards;

        public static GameUI Create(Transform parent)
        {
            var go = new GameObject("UI");
            go.transform.SetParent(parent, false);
            var ui = go.AddComponent<GameUI>();
            ui.Build();
            return ui;
        }

        private void Build()
        {
            font = Resources.Load<Font>("Fonts/Pixel57");
            if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(960, 540);
            scaler.matchWidthOrHeight = 0.5f;
            gameObject.AddComponent<GraphicRaycaster>();

            if (FindFirstObjectByType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                es.transform.SetParent(transform.parent, false);
            }

            hud = NewText("HUD", transform, "", 20, TextAnchor.UpperLeft);
            Stretch(hud.rectTransform, new Vector2(0, 0), new Vector2(1, 1), new Vector2(16, 12), new Vector2(-16, -12));

            shopPanel = NewPanel("Shop", transform);
            deathPanel = NewPanel("Death", transform);
            shopPanel.SetActive(false);
            deathPanel.SetActive(false);

            deathText = NewText("DeathText", deathPanel.transform, "", 32, TextAnchor.MiddleCenter);
            Stretch(deathText.rectTransform, new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);

            Palette.Changed += Recolor;
            Recolor();
        }

        private void OnDestroy() => Palette.Changed -= Recolor;

        private void Recolor()
        {
            foreach (var kv in tinted)
                if (kv.Key != null) kv.Key.color = kv.Value ? Palette.Foreground : Palette.Background;
        }

        // ---------- publiczne API ----------

        public void SetHud(int hp, int maxHp, int wave, int coins)
        {
            hud.text = "HP " + Mathf.Max(0, hp) + "/" + maxHp + "    WAVE " + wave + "    COINS " + coins;
        }

        public void ShowShop(List<Offer> offers, int coins, Action<Offer> onBuy, Action onSkip)
        {
            foreach (Transform c in shopPanel.transform) Destroy(c.gameObject);
            tinted.RemoveAll(kv => kv.Key == null);

            var title = NewText("Title", shopPanel.transform, "SHOP   (COINS: " + coins + ")", 30, TextAnchor.UpperCenter);
            Stretch(title.rectTransform, new Vector2(0, 0.85f), new Vector2(1, 1), Vector2.zero, Vector2.zero);

            for (int i = 0; i < offers.Count; i++)
            {
                var o = offers[i];
                float x0 = 0.04f + i * 0.32f;
                var card = NewImage("Card", shopPanel.transform, true);
                Stretch(card.rectTransform, new Vector2(x0, 0.25f), new Vector2(x0 + 0.28f, 0.8f), Vector2.zero, Vector2.zero);
                var inner = NewImage("Inner", card.transform, false);
                Stretch(inner.rectTransform, Vector2.zero, Vector2.one, new Vector2(3, 3), new Vector2(-3, -3));

                bool canBuy = coins >= o.cost;
                var txt = NewText("Text", inner.transform,
                    "GAIN\n" + o.gain.name + "\n" + o.gain.description + "\n\nPRICE\n" + o.price.name + "\n" + o.price.description +
                    "\n\n" + (canBuy ? "BUY: " + o.cost + " COINS" : "NEED " + o.cost + " COINS"),
                    18, TextAnchor.MiddleCenter);
                Stretch(txt.rectTransform, Vector2.zero, Vector2.one, new Vector2(8, 8), new Vector2(-8, -8));

                var iconSprite = string.IsNullOrEmpty(o.price.icon) ? null : Resources.Load<Sprite>("Icons/" + o.price.icon);
                if (iconSprite != null)
                {
                    var icon = NewImage("Icon", inner.transform, true);
                    icon.sprite = iconSprite;
                    icon.raycastTarget = false;
                    Stretch(icon.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(-24, -56), new Vector2(24, -8));
                }

                var btn = card.gameObject.AddComponent<Button>();
                btn.transition = Selectable.Transition.None;
                btn.targetGraphic = card;
                btn.interactable = canBuy;
                var offer = o;
                btn.onClick.AddListener(() => onBuy(offer));
            }

            var skip = NewImage("Skip", shopPanel.transform, true);
            Stretch(skip.rectTransform, new Vector2(0.35f, 0.08f), new Vector2(0.65f, 0.18f), Vector2.zero, Vector2.zero);
            var skipIn = NewImage("Inner", skip.transform, false);
            Stretch(skipIn.rectTransform, Vector2.zero, Vector2.one, new Vector2(3, 3), new Vector2(-3, -3));
            var st = NewText("Text", skipIn.transform, "SKIP", 20, TextAnchor.MiddleCenter);
            Stretch(st.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var sb = skip.gameObject.AddComponent<Button>();
            sb.transition = Selectable.Transition.None;
            sb.targetGraphic = skip;
            sb.onClick.AddListener(() => onSkip());

            Recolor();
            shopPanel.SetActive(true);
        }

        public void HideShop() => shopPanel.SetActive(false);

        public void ShowDeath(int wave, Action onRestart)
        {
            deathText.text = "GAME OVER\nYOU REACHED WAVE " + wave + "\n\n[ CLICK OR PRESS R ]";
            var btn = deathPanel.GetComponent<Button>() ?? deathPanel.AddComponent<Button>();
            btn.transition = Selectable.Transition.None;
            btn.targetGraphic = deathPanel.GetComponent<Image>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => onRestart());
            Recolor();
            deathPanel.SetActive(true);
        }

        public void HideDeath() => deathPanel.SetActive(false);

        // ---------- helpery ----------

        private GameObject NewPanel(string name, Transform parent)
        {
            var img = NewImage(name, parent, false);
            Stretch(img.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            return img.gameObject;
        }

        private Image NewImage(string name, Transform parent, bool foreground)
        {
            var img = new GameObject(name, typeof(RectTransform), typeof(Image)).GetComponent<Image>();
            img.rectTransform.SetParent(parent, false);
            tinted.Add(new KeyValuePair<Graphic, bool>(img, foreground));
            return img;
        }

        private Text NewText(string name, Transform parent, string text, int size, TextAnchor anchor)
        {
            var t = new GameObject(name, typeof(RectTransform), typeof(Text)).GetComponent<Text>();
            t.rectTransform.SetParent(parent, false);
            t.font = font;
            t.fontSize = size;
            t.alignment = anchor;
            t.text = text;
            t.raycastTarget = false;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            tinted.Add(new KeyValuePair<Graphic, bool>(t, true));
            return t;
        }

        private static void Stretch(RectTransform r, Vector2 aMin, Vector2 aMax, Vector2 oMin, Vector2 oMax)
        {
            r.anchorMin = aMin; r.anchorMax = aMax; r.offsetMin = oMin; r.offsetMax = oMax;
        }
    }
}
