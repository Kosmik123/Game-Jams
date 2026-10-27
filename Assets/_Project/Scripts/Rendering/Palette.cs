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

        /// <summary>Pary (ciemny, jasny) zmieniane co kilka fal.</summary>
        public static readonly string[][] Pairs =
        {
            new[] { "14121A", "F4F1DE" },
            new[] { "0F380F", "9BBC0F" },
            new[] { "000000", "FFFFFF" },
            new[] { "2B0A3D", "FF5E9C" },
            new[] { "1A0A00", "FFB000" },
        };

        /// <summary>Paleta glowna strefy (1-3) + zapasowe zmieniane co 2 fale w obrebie strefy.</summary>
        private static readonly string[][] ZoneMain =
        {
            new[] { "14121A", "F4F1DE" },   // Toll Road
            new[] { "0A1F1A", "E6C84A" },   // The Bank
            new[] { "2B0A3D", "FF5E9C" },   // Auction House
        };

        private static readonly string[][] Backups =
        {
            new[] { "0F380F", "9BBC0F" },
            new[] { "1A0A00", "FFB000" },
            new[] { "000000", "FFFFFF" },
        };

        public static void SetZone(int zone, int waveInZone)
        {
            int z = Mathf.Clamp(zone, 1, ZoneMain.Length) - 1;
            int group = Mathf.Max(0, waveInZone - 1) / 2;   // 0,0,1,1,2,2(boss)
            string[] p = group == 0 || waveInZone > 5 ? ZoneMain[z] : Backups[(z + group - 1) % Backups.Length];
            SetPair(p[0], p[1]);
        }

        public static void SetIndex(int index)
        {
            var p = Pairs[((index % Pairs.Length) + Pairs.Length) % Pairs.Length];
            SetPair(p[0], p[1]);
        }

        public static void Reset()
        {
            inverted = false;
            SetIndex(0);
        }

        private static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out var c);
            return c;
        }
    }
}
