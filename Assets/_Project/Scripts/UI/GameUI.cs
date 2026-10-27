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
        private GameObject shopPanel, deathPanel, titlePanel, zonePanel, pausePanel;
        private readonly List<RectTransform> cardRects = new List<RectTransform>();
        private float shopAnimStart;
        private bool shopAnimating;
        public bool AnimateNextShop { get; set; }
        private Text zoneText, toastText;
        private RectTransform priceHud;
        private string priceHudKey = "";
        private float toastTimer;
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

            priceHud = new GameObject("Price HUD", typeof(RectTransform)).GetComponent<RectTransform>();
            priceHud.SetParent(transform, false);
            priceHud.anchorMin = priceHud.anchorMax = new Vector2(1, 1);
            priceHud.pivot = new Vector2(1, 1);
            priceHud.sizeDelta = Vector2.zero;
            priceHud.anchoredPosition = new Vector2(-8, -40);

            hud = NewText("HUD", transform, "", 20, TextAnchor.UpperLeft);
            Stretch(hud.rectTransform, new Vector2(0, 0), new Vector2(1, 1), new Vector2(16, 12), new Vector2(-16, -12));

            shopPanel = NewPanel("Shop", transform);
            deathPanel = NewPanel("Death", transform);
            shopPanel.SetActive(false);
            deathPanel.SetActive(false);
            BuildTitle();

            zonePanel = NewPanel("Zone", transform);
            zonePanel.GetComponent<Image>().raycastTarget = false;   // ekran strefy nigdy nie blokuje klikniec
            zoneText = NewText("ZoneText", zonePanel.transform, "", 44, TextAnchor.MiddleCenter);
            Stretch(zoneText.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            zonePanel.SetActive(false);

            pausePanel = NewPanel("Pause", transform);
            pausePanel.SetActive(false);

            toastText = NewText("Toast", transform, "", 22, TextAnchor.UpperCenter);
            Stretch(toastText.rectTransform, new Vector2(0, 0.80f), new Vector2(1, 0.9f), Vector2.zero, Vector2.zero);


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
            if (bossLinePanel != null && bossLinePanel.activeSelf && Time.unscaledTime > bossLineUntil) bossLinePanel.SetActive(false);
            if (logoFrames != null && titlePanel.activeSelf)
            {
                // klatka 0 spokojna, 1-2 to glitch: ~0.12 s co kilka sekund
                float cycle = Time.unscaledTime % 3.2f;
                int f = cycle > 2.9f ? 1 : cycle > 2.78f ? 2 : 0;
                if (logoImage.sprite != logoFrames[f]) logoImage.sprite = logoFrames[f];
            }

            if (shopAnimating && shopPanel.activeSelf)
            {
                float t = Time.unscaledTime - shopAnimStart;
                bool done = true;
                for (int i = 0; i < cardRects.Count; i++)
                {
                    if (cardRects[i] == null) continue;
                    float k = Mathf.Clamp01((t - i * 0.1f) / 0.25f);
                    float ease = 1f - (1f - k) * (1f - k) * (1f - k);
                    cardRects[i].anchoredPosition = new Vector2(0, -320f * (1f - ease));
                    if (k < 1f) done = false;
                }
                if (done) shopAnimating = false;
            }

            if (toastTimer > 0)
            {
                toastTimer -= Time.deltaTime;
                if (toastTimer <= 0) toastText.text = "";
            }
        }

        public void Toast(string msg, float seconds = 2.5f)
        {
            toastText.text = msg;
            toastTimer = seconds;
        }

        /// <summary>Lista zaplaconych cen: ikona + licznik stackow (prawy gorny rog).</summary>
        public void SetPriceHud(List<KeyValuePair<string, int>> entries)
        {
            var sb = new System.Text.StringBuilder();
            foreach (var e in entries) sb.Append(e.Key).Append(':').Append(e.Value).Append(';');
            if (sb.ToString() == priceHudKey) return;
            priceHudKey = sb.ToString();

            foreach (Transform c in priceHud) Destroy(c.gameObject);
            tinted.RemoveAll(kv => kv.Key == null);
            for (int i = 0; i < entries.Count; i++)
            {
                var sprite = Resources.Load<Sprite>("Icons/" + entries[i].Key);
                var holder = new GameObject("Entry", typeof(RectTransform)).GetComponent<RectTransform>();
                holder.SetParent(priceHud, false);
                holder.anchorMin = holder.anchorMax = new Vector2(1, 1);
                holder.pivot = new Vector2(1, 1);
                holder.sizeDelta = new Vector2(44, 44);
                holder.anchoredPosition = new Vector2(-i * 48f, 0);

                if (sprite != null)
                {
                    var icon = NewImage("Icon", holder, true);
                    icon.sprite = sprite;
                    icon.raycastTarget = false;
                    Stretch(icon.rectTransform, Vector2.zero, Vector2.one, new Vector2(4, 12), new Vector2(-4, -4));
                }
                var count = NewText("Count", holder, "x" + entries[i].Value, 14, TextAnchor.LowerCenter);
                Stretch(count.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            }
            Recolor();
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
            int rerollCost, Action onReroll, bool haggleAvailable, Action onHaggle, Action onPayDebt,
            string contractLabel = null, Action onContract = null, string signedLabel = null)
        {
            HideZone();
            HideTitle();
            shopPanel.transform.SetAsLastSibling();
            toastText.transform.SetAsLastSibling();
            foreach (Transform c in shopPanel.transform) Destroy(c.gameObject);
            tinted.RemoveAll(kv => kv.Key == null);
            cardRects.Clear();
            shopAnimating = AnimateNextShop;
            AnimateNextShop = false;
            shopAnimStart = Time.unscaledTime;

            var title = NewText("Title", shopPanel.transform, "SHOP   COINS: " + coins + (debt > 0 ? "   DEBT: " + debt : ""), 30, TextAnchor.UpperCenter);
            Stretch(title.rectTransform, new Vector2(0, 0.85f), new Vector2(1, 1), Vector2.zero, Vector2.zero);

            for (int i = 0; i < offers.Count; i++)
            {
                var o = offers[i];
                float x0 = 0.04f + i * 0.32f;
                var card = NewImage("Card", shopPanel.transform, true);
                Stretch(card.rectTransform, new Vector2(x0, 0.25f), new Vector2(x0 + 0.28f, 0.8f), Vector2.zero, Vector2.zero);
                cardRects.Add(card.rectTransform);
                if (shopAnimating) card.rectTransform.anchoredPosition = new Vector2(0, -320f);
                var inner = NewImage("Inner", card.transform, false);
                Stretch(inner.rectTransform, Vector2.zero, Vector2.one, new Vector2(3, 3), new Vector2(-3, -3));

                bool canBuy = Game.Instance.CanAfford(o);
                int cost = Game.Instance.CostOf(o);
                bool onCredit = !o.sell && coins < cost;
                string top = o.sell
                    ? "SELL\nGET " + o.reward + " COINS"
                    : "GAIN" + (o.level > 1 ? " LV" + o.level : "") + "\n" + o.gain.name + "\n" + o.gain.description;
                string key = "[" + (i + 1) + "] ";
                string bottom = o.sold ? "* SOLD *"
                    : o.sell ? key + "SELL"
                    : o.free ? key + "FIRST ONE IS FREE!"
                    : !canBuy ? "NEED " + cost + " COINS"
                    : onCredit ? key + "CREDIT: DEBT +" + (cost - coins)
                    : key + "BUY: " + cost + " COINS";
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

            MakeShopButton("[R] REROLL: " + rerollCost, 0.02f, 0.25f, coins >= rerollCost, onReroll);
            MakeShopButton(debt > 0 ? "[D] PAY DEBT: " + Mathf.Min(coins, debt) : "NO DEBT", 0.26f, 0.49f, debt > 0 && coins > 0, onPayDebt);
            MakeShopButton(haggleAvailable ? "[H] HAGGLE: 1" : "HAGGLED", 0.50f, 0.73f, haggleAvailable && coins >= 1, onHaggle);
            MakeShopButton("[ENTER] NEXT WAVE", 0.74f, 0.98f, true, onSkip);
            if (contractLabel != null)
                MakeButton(shopPanel.transform, "[C] CONTRACT - " + contractLabel, new Vector2(0.02f, 0.19f), new Vector2(0.98f, 0.25f), true, onContract);
            if (signedLabel != null)
            {
                var st = NewText("Signed", shopPanel.transform, signedLabel, 16, TextAnchor.MiddleCenter);
                Stretch(st.rectTransform, new Vector2(0, 0.0f), new Vector2(1, 0.07f), Vector2.zero, Vector2.zero);
            }

            Recolor();
            shopPanel.SetActive(true);
        }

        private void MakeShopButton(string label, float x0, float x1, bool interactable, Action onClick) =>
            MakeButton(shopPanel.transform, label, new Vector2(x0, 0.08f), new Vector2(x1, 0.18f), interactable, onClick);

        private void MakeButton(Transform parent, string label, Vector2 aMin, Vector2 aMax, bool interactable, Action onClick)
        {
            var b = NewImage("Button", parent, true);
            Stretch(b.rectTransform, aMin, aMax, Vector2.zero, Vector2.zero);
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

        // ---------- pauza ----------

        public void ShowPause(Action onResume, Action onRestart, Action onVolume, Action onFlicker, Action onShake, Action onMusic, Action onSfx)
        {
            foreach (Transform c in pausePanel.transform) Destroy(c.gameObject);
            tinted.RemoveAll(kv => kv.Key == null);
            pausePanel.transform.SetAsLastSibling();

            var title = NewText("Title", pausePanel.transform, "PAUSED", 40, TextAnchor.MiddleCenter);
            Stretch(title.rectTransform, new Vector2(0, 0.78f), new Vector2(1, 0.95f), Vector2.zero, Vector2.zero);

            int vol = Mathf.RoundToInt(Settings.Volume * 10f);
            int mus = Mathf.RoundToInt(Settings.MusicVolume * 10f);
            int sfx = Mathf.RoundToInt(Settings.SfxVolume * 10f);
            MakeButton(pausePanel.transform, "RESUME", new Vector2(0.3f, 0.66f), new Vector2(0.7f, 0.75f), true, onResume);
            MakeButton(pausePanel.transform, "MASTER " + vol + "/10  (CLICK +1)", new Vector2(0.3f, 0.56f), new Vector2(0.7f, 0.65f), true, onVolume);
            MakeButton(pausePanel.transform, "MUSIC " + mus + "/10  (CLICK +1)", new Vector2(0.3f, 0.46f), new Vector2(0.7f, 0.55f), true, onMusic);
            MakeButton(pausePanel.transform, "SFX " + sfx + "/10  (CLICK +1)", new Vector2(0.3f, 0.36f), new Vector2(0.7f, 0.45f), true, onSfx);
            MakeButton(pausePanel.transform, "REDUCE FLICKER: " + (Settings.ReduceFlicker ? "ON" : "OFF"), new Vector2(0.3f, 0.26f), new Vector2(0.7f, 0.35f), true, onFlicker);
            MakeButton(pausePanel.transform, "SCREENSHAKE: " + (Settings.Shake ? "ON" : "OFF"), new Vector2(0.3f, 0.16f), new Vector2(0.7f, 0.25f), true, onShake);
            MakeButton(pausePanel.transform, "RESTART RUN", new Vector2(0.3f, 0.06f), new Vector2(0.7f, 0.15f), true, onRestart);

            Recolor();
            pausePanel.SetActive(true);
            toastText.transform.SetAsLastSibling();
        }

        public void HidePause() => pausePanel.SetActive(false);

        private RectTransform titleDynamic;
        private Sprite[] logoFrames;
        private Image logoImage;

        private void BuildTitle()
        {
            titlePanel = NewPanel("Title", transform);
            titlePanel.transform.SetAsLastSibling();

            var logoSprite = Resources.Load<Sprite>("Sprites/title_logo");
            if (logoSprite != null)
            {
                var logo = NewImage("Logo", titlePanel.transform, true);
                logo.sprite = logoSprite;
                var f0 = Resources.Load<Sprite>("Sprites/title_logo_0");
                var f1 = Resources.Load<Sprite>("Sprites/title_logo_1");
                var f2 = Resources.Load<Sprite>("Sprites/title_logo_2");
                if (f0 != null && f1 != null && f2 != null)
                {
                    logoFrames = new[] { f0, f1, f2 };
                    logoImage = logo;
                    logo.sprite = f0;
                }
                logo.preserveAspect = true;
                logo.raycastTarget = false;
                Stretch(logo.rectTransform, new Vector2(0.1f, 0.60f), new Vector2(0.9f, 0.97f), Vector2.zero, Vector2.zero);
            }
            else
            {
                var t = NewText("Name", titlePanel.transform, "PAY WITH PIXELS", 48, TextAnchor.MiddleCenter);
                Stretch(t.rectTransform, new Vector2(0, 0.62f), new Vector2(1, 0.95f), Vector2.zero, Vector2.zero);
            }

            var tagSprite = Resources.Load<Sprite>("Sprites/title_tagline");
            if (tagSprite != null)
            {
                var tag = NewImage("Tagline", titlePanel.transform, true);
                tag.sprite = tagSprite;
                tag.preserveAspect = true;
                tag.raycastTarget = false;
                Stretch(tag.rectTransform, new Vector2(0.2f, 0.50f), new Vector2(0.8f, 0.60f), Vector2.zero, Vector2.zero);
            }

            titleDynamic = new GameObject("Dynamic", typeof(RectTransform)).GetComponent<RectTransform>();
            titleDynamic.SetParent(titlePanel.transform, false);
            Stretch(titleDynamic, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            titlePanel.SetActive(false);
        }

        /// <summary>Ekran tytulowy: wybor broni, osiagniecia, rekord. Klik poza przyciskami = start.</summary>
        public void ShowTitle(Action onStart, Action<Weapon> onWeapon, Action onAscension)
        {
            foreach (Transform c in titleDynamic) Destroy(c.gameObject);
            tinted.RemoveAll(kv => kv.Key == null);

            // bron
            var weapons = new[] { Weapon.Pistol, Weapon.Shotgun, Weapon.Laser };
            string[] names = { "PISTOL", "SHOTGUN", "LASER" };
            for (int i = 0; i < weapons.Length; i++)
            {
                var w = weapons[i];
                bool open = Meta.IsWeaponUnlocked(w);
                string label = open ? (Meta.Selected == w ? "> " + names[i] + " <" : "[" + (i + 1) + "] " + names[i])
                                    : "LOCKED: " + Meta.WeaponUnlockHint(w);
                float x0 = 0.06f + i * 0.31f;
                MakeButton(titleDynamic, label, new Vector2(x0, 0.37f), new Vector2(x0 + 0.29f, 0.47f), open, () => onWeapon(w));
            }

            // osiagniecia
            int unlockedCount = 0;
            for (int i = 0; i < Meta.Achievements.Length; i++)
            {
                var a = Meta.Achievements[i];
                bool has = Meta.Has(a.id);
                if (has) unlockedCount++;
                var sprite = Resources.Load<Sprite>("Icons/" + (has ? a.icon : "ach_locked"));
                if (sprite == null) continue;
                var icon = NewImage("Ach", titleDynamic, true);
                icon.sprite = sprite;
                icon.raycastTarget = false;
                float x0 = 0.5f - Meta.Achievements.Length * 0.0325f + i * 0.065f;
                Stretch(icon.rectTransform, new Vector2(x0, 0.27f), new Vector2(x0 + 0.06f, 0.36f), Vector2.zero, Vector2.zero);
                icon.preserveAspect = true;
            }
            var achText = NewText("AchText", titleDynamic, "ACHIEVEMENTS " + unlockedCount + "/" + Meta.Achievements.Length, 16, TextAnchor.MiddleCenter);
            Stretch(achText.rectTransform, new Vector2(0, 0.22f), new Vector2(1, 0.27f), Vector2.zero, Vector2.zero);

            if (Meta.AscensionUnlocked > 0)
                MakeButton(titleDynamic, "ASCENSION " + Meta.Ascension + "/" + Meta.AscensionUnlocked + "  [CLICK TO CHANGE]  SCORE x" + (1f + 0.25f * Meta.Ascension).ToString("0.00"),
                    new Vector2(0.2f, 0.135f), new Vector2(0.8f, 0.215f), true, onAscension);

            var start = NewText("Start", titleDynamic,
                "CLICK OR PRESS SPACE TO START\nBEST WAVE " + Meta.BestWave + "   BEST SCORE " + Meta.BestScore + "   WINS " + Meta.Wins, 20, TextAnchor.MiddleCenter);
            Stretch(start.rectTransform, new Vector2(0, 0.01f), new Vector2(1, 0.13f), Vector2.zero, Vector2.zero);

            var btn = titlePanel.GetComponent<Button>() ?? titlePanel.AddComponent<Button>();
            btn.transition = Selectable.Transition.None;
            btn.targetGraphic = titlePanel.GetComponent<Image>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => onStart());
            titlePanel.transform.SetAsLastSibling();
            toastText.transform.SetAsLastSibling();
            Recolor();
            titlePanel.SetActive(true);
        }

        public void HideTitle() => titlePanel.SetActive(false);

        /// <summary>Koniec runu: naglowek, statystyki i przyciski (jeszcze raz / tytul).</summary>
        public void ShowDeath(bool won, string details, Action onAgain, Action onTitle)
        {
            foreach (Transform c in deathPanel.transform) Destroy(c.gameObject);
            tinted.RemoveAll(kv => kv.Key == null);
            deathPanel.transform.SetAsLastSibling();
            toastText.transform.SetAsLastSibling();

            var head = NewText("Head", deathPanel.transform, won ? "SOLD" : "PAID IN FULL", 48, TextAnchor.MiddleCenter);
            Stretch(head.rectTransform, new Vector2(0, 0.76f), new Vector2(1, 0.96f), Vector2.zero, Vector2.zero);
            var ending = Resources.Load<Sprite>(won ? "Story/story_ending_sold" : "Story/story_ending_paid");
            bool hasArt = ending != null;
            if (hasArt)
            {
                var art = NewImage("Ending", deathPanel.transform, true);
                art.sprite = ending;
                art.preserveAspect = true;
                art.raycastTarget = false;
                Stretch(art.rectTransform, new Vector2(0.03f, 0.3f), new Vector2(0.45f, 0.75f), Vector2.zero, Vector2.zero);
            }
            var body = NewText("Body", deathPanel.transform, details, hasArt ? 18 : 20, hasArt ? TextAnchor.UpperLeft : TextAnchor.UpperCenter);
            if (hasArt) Stretch(body.rectTransform, new Vector2(0.47f, 0.27f), new Vector2(0.98f, 0.75f), Vector2.zero, Vector2.zero);
            else Stretch(body.rectTransform, new Vector2(0.1f, 0.34f), new Vector2(0.9f, 0.74f), Vector2.zero, Vector2.zero);

            MakeButton(deathPanel.transform, "[R] AGAIN", new Vector2(0.15f, 0.12f), new Vector2(0.48f, 0.24f), true, onAgain);
            MakeButton(deathPanel.transform, "[T] TITLE", new Vector2(0.52f, 0.12f), new Vector2(0.85f, 0.24f), true, onTitle);

            Recolor();
            deathPanel.SetActive(true);
        }

        public void HideDeath() => deathPanel.SetActive(false);

        // ---------- fabula ----------

        private GameObject storyPanel, bossLinePanel;
        private Image storyImage;
        private Text storyText, storyHint, bossLineText;
        private float bossLineUntil;

        /// <summary>Pelnoekranowy slajd fabularny (kadr z Resources/Story + podpis). Brak kadru = sam tekst.</summary>
        public void ShowStory(string sprite, string text, int index, int total)
        {
            if (storyPanel == null)
            {
                storyPanel = NewPanel("Story", transform);
                storyImage = NewImage("Frame", storyPanel.transform, true);
                storyImage.raycastTarget = false;
                storyImage.preserveAspect = true;
                Stretch(storyImage.rectTransform, new Vector2(0.1f, 0.3f), new Vector2(0.9f, 0.95f), Vector2.zero, Vector2.zero);
                storyText = NewText("Text", storyPanel.transform, "", 24, TextAnchor.MiddleCenter);
                Stretch(storyText.rectTransform, new Vector2(0.05f, 0.12f), new Vector2(0.95f, 0.3f), Vector2.zero, Vector2.zero);
                storyHint = NewText("Hint", storyPanel.transform, "", 16, TextAnchor.MiddleCenter);
                Stretch(storyHint.rectTransform, new Vector2(0, 0.01f), new Vector2(1, 0.1f), Vector2.zero, Vector2.zero);
            }
            var s = Resources.Load<Sprite>("Story/" + sprite);
            storyImage.sprite = s;
            storyImage.enabled = s != null;
            storyText.text = text;
            storyHint.text = index + "/" + total + "   CLICK / SPACE: NEXT    ESC: SKIP";
            storyPanel.transform.SetAsLastSibling();
            Recolor();
            storyPanel.SetActive(true);
        }

        public void HideStory() { if (storyPanel != null) storyPanel.SetActive(false); }

        /// <summary>Linijka bossa w ramce dialogu na dole ekranu (znika sama).</summary>
        public void ShowBossLine(string line)
        {
            if (bossLinePanel == null)
            {
                var frame = NewImage("BossLine", transform, true);
                var sprite = Resources.Load<Sprite>("UI/ui_dialog_frame");
                frame.sprite = sprite;
                if (sprite != null && sprite.border != Vector4.zero) frame.type = Image.Type.Sliced;
                frame.raycastTarget = false;
                Stretch(frame.rectTransform, new Vector2(0.08f, 0.04f), new Vector2(0.92f, 0.2f), Vector2.zero, Vector2.zero);
                bossLinePanel = frame.gameObject;
                var bg = NewImage("Bg", bossLinePanel.transform, false);
                bg.raycastTarget = false;
                Stretch(bg.rectTransform, Vector2.zero, Vector2.one, new Vector2(3, 3), new Vector2(-3, -3));
                bossLineText = NewText("Text", bossLinePanel.transform, "", 20, TextAnchor.MiddleCenter);
                Stretch(bossLineText.rectTransform, Vector2.zero, Vector2.one, new Vector2(12, 8), new Vector2(-12, -8));
            }
            bossLineText.text = line;
            bossLineUntil = Time.unscaledTime + 2.5f;
            bossLinePanel.transform.SetAsLastSibling();
            toastText.transform.SetAsLastSibling();
            Recolor();
            bossLinePanel.SetActive(true);
        }

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
