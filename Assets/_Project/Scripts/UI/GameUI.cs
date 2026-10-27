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
        private GameObject shopPanel, deathPanel, titlePanel, zonePanel;
        private Text zoneText, toastText;
        private float toastTimer;
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
            BuildTitle();

            zonePanel = NewPanel("Zone", transform);
            zoneText = NewText("ZoneText", zonePanel.transform, "", 44, TextAnchor.MiddleCenter);
            Stretch(zoneText.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            zonePanel.SetActive(false);

            toastText = NewText("Toast", transform, "", 22, TextAnchor.UpperCenter);
            Stretch(toastText.rectTransform, new Vector2(0, 0.80f), new Vector2(1, 0.9f), Vector2.zero, Vector2.zero);

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

        private void Update()
        {
            if (toastTimer > 0)
            {
                toastTimer -= Time.deltaTime;
                if (toastTimer <= 0) toastText.text = "";
            }
        }

        public void Toast(string msg)
        {
            toastText.text = msg;
            toastTimer = 2.5f;
        }

        public void ShowZone(int zone, string name)
        {
            zoneText.text = "ZONE " + zone + "\n" + name;
            zonePanel.transform.SetAsLastSibling();
            zonePanel.SetActive(true);
        }

        public void HideZone() => zonePanel.SetActive(false);

        public void SetHud(int hp, int maxHp, string zoneLabel, int coins, int debt, string extra = "")
        {
            hud.text = "HP " + Mathf.Max(0, hp) + "/" + maxHp + "    " + zoneLabel + "    COINS " + coins + (debt > 0 ? "    DEBT " + debt : "") + (string.IsNullOrEmpty(extra) ? "" : "    " + extra) + "    [M] SOUND " + (Sfx.Muted ? "OFF" : "ON");
        }

        public void ShowShop(List<Offer> offers, int coins, int debt, Action<Offer> onBuy, Action onSkip,
            int rerollCost, Action onReroll, bool haggleAvailable, Action onHaggle, Action onPayDebt)
        {
            foreach (Transform c in shopPanel.transform) Destroy(c.gameObject);
            tinted.RemoveAll(kv => kv.Key == null);

            var title = NewText("Title", shopPanel.transform, "SHOP   COINS: " + coins + (debt > 0 ? "   DEBT: " + debt : ""), 30, TextAnchor.UpperCenter);
            Stretch(title.rectTransform, new Vector2(0, 0.85f), new Vector2(1, 1), Vector2.zero, Vector2.zero);

            for (int i = 0; i < offers.Count; i++)
            {
                var o = offers[i];
                float x0 = 0.04f + i * 0.32f;
                var card = NewImage("Card", shopPanel.transform, true);
                Stretch(card.rectTransform, new Vector2(x0, 0.25f), new Vector2(x0 + 0.28f, 0.8f), Vector2.zero, Vector2.zero);
                var inner = NewImage("Inner", card.transform, false);
                Stretch(inner.rectTransform, Vector2.zero, Vector2.one, new Vector2(3, 3), new Vector2(-3, -3));

                bool canBuy = Game.Instance.CanAfford(o);
                int cost = Game.Instance.CostOf(o);
                bool onCredit = !o.sell && coins < cost;
                string top = o.sell
                    ? "SELL\nGET " + o.reward + " COINS"
                    : "GAIN" + (o.level > 1 ? " LV" + o.level : "") + "\n" + o.gain.name + "\n" + o.gain.description;
                string bottom = o.sold ? "* SOLD *"
                    : o.sell ? "[ SELL ]"
                    : !canBuy ? "NEED " + cost + " COINS"
                    : onCredit ? "CREDIT: DEBT +" + (cost - coins)
                    : "BUY: " + cost + " COINS";
                var txt = NewText("Text", inner.transform,
                    top + "\n\nPRICE\n" + o.price.name + "\n" + o.price.description +
                    (Catalog.SynergyText(o, Game.Instance.stats) is string syn ? "\n\n* SYNERGY *\n" + syn : "") + "\n\n" + bottom,
                    18, TextAnchor.MiddleCenter);
                Stretch(txt.rectTransform, Vector2.zero, Vector2.one, new Vector2(8, 8), new Vector2(-8, -8));

                var iconSprite = string.IsNullOrEmpty(o.price.icon) ? null : Resources.Load<Sprite>("Icons/" + o.price.icon);
                if (iconSprite != null)
                {
                    var icon = NewImage("Icon", inner.transform, true);
                    icon.sprite = iconSprite;
                    icon.raycastTarget = false;
                    Stretch(icon.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-56, -56), new Vector2(-8, -8));
                }

                var gainSprite = o.gain == null || string.IsNullOrEmpty(o.gain.icon) ? null : Resources.Load<Sprite>("Icons/" + o.gain.icon);
                if (gainSprite != null)
                {
                    var gi = NewImage("GainIcon", inner.transform, true);
                    gi.sprite = gainSprite;
                    gi.raycastTarget = false;
                    Stretch(gi.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(8, -56), new Vector2(56, -8));
                }

                var btn = card.gameObject.AddComponent<Button>();
                btn.transition = Selectable.Transition.None;
                btn.targetGraphic = card;
                btn.interactable = canBuy;
                var offer = o;
                btn.onClick.AddListener(() => onBuy(offer));
            }

            MakeShopButton("REROLL: " + rerollCost, 0.02f, 0.25f, coins >= rerollCost, onReroll);
            MakeShopButton(debt > 0 ? "PAY DEBT: " + Mathf.Min(coins, debt) : "NO DEBT", 0.26f, 0.49f, debt > 0 && coins > 0, onPayDebt);
            MakeShopButton(haggleAvailable ? "HAGGLE: 1" : "HAGGLED", 0.50f, 0.73f, haggleAvailable && coins >= 1, onHaggle);
            MakeShopButton("NEXT WAVE", 0.74f, 0.98f, true, onSkip);

            Recolor();
            shopPanel.SetActive(true);
        }

        private void MakeShopButton(string label, float x0, float x1, bool interactable, Action onClick)
        {
            var b = NewImage("Button", shopPanel.transform, true);
            Stretch(b.rectTransform, new Vector2(x0, 0.08f), new Vector2(x1, 0.18f), Vector2.zero, Vector2.zero);
            var inner = NewImage("Inner", b.transform, false);
            Stretch(inner.rectTransform, Vector2.zero, Vector2.one, new Vector2(3, 3), new Vector2(-3, -3));
            var t = NewText("Text", inner.transform, label, 18, TextAnchor.MiddleCenter);
            Stretch(t.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var btn = b.gameObject.AddComponent<Button>();
            btn.transition = Selectable.Transition.None;
            btn.targetGraphic = b;
            btn.interactable = interactable;
            btn.onClick.AddListener(() => onClick());
        }

        public void HideShop() => shopPanel.SetActive(false);

        private void BuildTitle()
        {
            titlePanel = NewPanel("Title", transform);
            titlePanel.transform.SetAsLastSibling();

            var logoSprite = Resources.Load<Sprite>("Sprites/title_logo");
            if (logoSprite != null)
            {
                var logo = NewImage("Logo", titlePanel.transform, true);
                logo.sprite = logoSprite;
                logo.preserveAspect = true;
                logo.raycastTarget = false;
                Stretch(logo.rectTransform, new Vector2(0.1f, 0.5f), new Vector2(0.9f, 0.92f), Vector2.zero, Vector2.zero);
            }
            else
            {
                var t = NewText("Name", titlePanel.transform, "PAY WITH PIXELS", 48, TextAnchor.MiddleCenter);
                Stretch(t.rectTransform, new Vector2(0, 0.55f), new Vector2(1, 0.9f), Vector2.zero, Vector2.zero);
            }

            var tagSprite = Resources.Load<Sprite>("Sprites/title_tagline");
            if (tagSprite != null)
            {
                var tag = NewImage("Tagline", titlePanel.transform, true);
                tag.sprite = tagSprite;
                tag.preserveAspect = true;
                tag.raycastTarget = false;
                Stretch(tag.rectTransform, new Vector2(0.2f, 0.32f), new Vector2(0.8f, 0.48f), Vector2.zero, Vector2.zero);
            }

            var start = NewText("Start", titlePanel.transform, "CLICK TO START\nWASD MOVE   MOUSE AIM + SHOOT", 22, TextAnchor.MiddleCenter);
            Stretch(start.rectTransform, new Vector2(0, 0.08f), new Vector2(1, 0.28f), Vector2.zero, Vector2.zero);
            titlePanel.SetActive(false);
        }

        public void ShowTitle(Action onStart)
        {
            var btn = titlePanel.GetComponent<Button>() ?? titlePanel.AddComponent<Button>();
            btn.transition = Selectable.Transition.None;
            btn.targetGraphic = titlePanel.GetComponent<Image>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => onStart());
            Recolor();
            titlePanel.SetActive(true);
        }

        public void HideTitle() => titlePanel.SetActive(false);

        public void ShowDeath(int wave, Action onRestart, bool won = false)
        {
            deathText.text = (won ? "YOU WIN\nTHE BOSS PAID THE PRICE" : "GAME OVER\nYOU REACHED WAVE " + wave) + "\n\n[ CLICK OR PRESS R ]";
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
