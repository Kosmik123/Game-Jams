using System;
using UnityEngine;

namespace PayWithPixels
{
    /// <summary>Aktualna para kolorow gry. Cala gra (RT + UI) uzywa tylko tych dwoch.</summary>
    public static class Palette
    {
        public static event Action Changed;

        private static Color dark = Hex("14121A");
        private static Color light = Hex("F4F1DE");
        private static bool inverted;

        public static Color Background => inverted ? light : dark;
        public static Color Foreground => inverted ? dark : light;

        public static bool Inverted
        {
            get => inverted;
            set { if (inverted == value) return; inverted = value; Changed?.Invoke(); }
        }

        public static void SetPair(string darkHex, string lightHex)
        {
            dark = Hex(darkHex);
            light = Hex(lightHex);
            Changed?.Invoke();
        }

        public static void Reset()
        {
            inverted = false;
            SetPair("14121A", "F4F1DE");
        }

        private static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out var c);
            return c;
        }
    }
}
