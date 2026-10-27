using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace PayWithPixels
{
    /// <summary>
    /// Trwaly zapis gry. W WebGL localStorage pod stalymi kluczami (przetrwa nowy upload na itch.io), wszedzie indziej PlayerPrefs.
    /// W WebGL zapisujemy tez do PlayerPrefs (zapas, gdyby localStorage byl zablokowany) i z niego migrujemy stare dane.
    /// Kod zapisu (EXPORT/IMPORT) pozwala przeniesc postep miedzy przegladarkami.
    /// </summary>
    public static class Save
    {
        private const string Prefix = "payWithPixels_v1_";
        private const string CodeTag = "PWP1:";
        private static readonly Dictionary<string, char> known = new Dictionary<string, char>();   // klucz -> typ (i, f, s)

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern int PwpHas(string key);
        [DllImport("__Internal")] private static extern string PwpGet(string key);
        [DllImport("__Internal")] private static extern void PwpSet(string key, string value);
        [DllImport("__Internal")] private static extern void PwpRemove(string key);
        [DllImport("__Internal")] private static extern string PwpPrompt(string message, string def);

        private static bool Has(string key) { try { return PwpHas(Prefix + key) == 1; } catch { return false; } }
        private static string Raw(string key) { try { return PwpGet(Prefix + key); } catch { return ""; } }
        private static void Put(string key, string value) { try { PwpSet(Prefix + key, value); } catch { } }
        private static void Remove(string key) { try { PwpRemove(Prefix + key); } catch { } }
#else
        private static bool Has(string key) => false;
        private static string Raw(string key) => "";
        private static void Put(string key, string value) { }
        private static void Remove(string key) { }
#endif

        public static int GetInt(string key, int def)
        {
            known[key] = 'i';
            if (Has(key) && int.TryParse(Raw(key), NumberStyles.Integer, CultureInfo.InvariantCulture, out int v)) return v;
            return PlayerPrefs.GetInt(key, def);
        }

        public static float GetFloat(string key, float def)
        {
            known[key] = 'f';
            if (Has(key) && float.TryParse(Raw(key), NumberStyles.Float, CultureInfo.InvariantCulture, out float v)) return v;
            return PlayerPrefs.GetFloat(key, def);
        }

        public static string GetString(string key, string def)
        {
            known[key] = 's';
            return Has(key) ? Raw(key) : PlayerPrefs.GetString(key, def);
        }

        public static void SetInt(string key, int value) { known[key] = 'i'; PlayerPrefs.SetInt(key, value); Put(key, value.ToString(CultureInfo.InvariantCulture)); }
        public static void SetFloat(string key, float value) { known[key] = 'f'; PlayerPrefs.SetFloat(key, value); Put(key, value.ToString("R", CultureInfo.InvariantCulture)); }
        public static void SetString(string key, string value) { known[key] = 's'; PlayerPrefs.SetString(key, value); Put(key, value); }

        public static void DeleteKey(string key) { PlayerPrefs.DeleteKey(key); Remove(key); }

        public static void Flush() => PlayerPrefs.Save();

        // ---------- kod zapisu ----------

        /// <summary>Kod zapisu: PWP1: + base64 linii "klucz=typ:wartosc" (typy i, f, s).</summary>
        public static string Export()
        {
            Meta.Preload();
            Settings.Preload();
            var sb = new StringBuilder();
            foreach (var kv in new Dictionary<string, char>(known))
            {
                string val;
                switch (kv.Value)
                {
                    case 'i': val = GetInt(kv.Key, 0).ToString(CultureInfo.InvariantCulture); break;
                    case 'f': val = GetFloat(kv.Key, 0f).ToString("R", CultureInfo.InvariantCulture); break;
                    default: val = GetString(kv.Key, "").Replace("\n", " "); break;
                }
                sb.Append(kv.Key).Append('=').Append(kv.Value).Append(':').Append(val).Append('\n');
            }
            return CodeTag + Convert.ToBase64String(Encoding.UTF8.GetBytes(sb.ToString()));
        }

        /// <summary>Wczytuje kod zapisu; zwraca false, gdy kod jest nieprawidlowy (nic wtedy nie zmienia).</summary>
        public static bool Import(string code)
        {
            if (string.IsNullOrEmpty(code)) return false;
            code = code.Trim();
            if (!code.StartsWith(CodeTag, StringComparison.Ordinal)) return false;
            string text;
            try { text = Encoding.UTF8.GetString(Convert.FromBase64String(code.Substring(CodeTag.Length))); }
            catch { return false; }

            var entries = new List<string[]>();
            foreach (var line in text.Split('\n'))
            {
                int eq = line.IndexOf('=');
                if (eq <= 0 || line.Length < eq + 3 || line[eq + 2] != ':') continue;
                string key = line.Substring(0, eq);
                if (!key.StartsWith("pwp_", StringComparison.Ordinal)) continue;
                entries.Add(new[] { key, line[eq + 1].ToString(), line.Substring(eq + 3) });
            }
            if (entries.Count == 0) return false;
            foreach (var e in entries)
            {
                switch (e[1][0])
                {
                    case 'i': if (int.TryParse(e[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int i)) SetInt(e[0], i); break;
                    case 'f': if (float.TryParse(e[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float f)) SetFloat(e[0], f); break;
                    case 's': SetString(e[0], e[2]); break;
                }
            }
            Flush();
            Meta.Reload();
            Settings.Reload();
            return true;
        }

        /// <summary>Pokazuje kod do skopiowania (okno przegladarki w WebGL, schowek wszedzie). Zwraca kod.</summary>
        public static string ShowExport()
        {
            string code = Export();
            try { GUIUtility.systemCopyBuffer = code; } catch { }
#if UNITY_WEBGL && !UNITY_EDITOR
            try { PwpPrompt("SAVE CODE (Ctrl+C to copy):", code); } catch { }
#endif
            return code;
        }

        /// <summary>Prosi o kod zapisu (okno przegladarki w WebGL, schowek wszedzie) i go wczytuje.</summary>
        public static bool AskImport()
        {
            string code;
#if UNITY_WEBGL && !UNITY_EDITOR
            try { code = PwpPrompt("PASTE SAVE CODE:", ""); } catch { code = ""; }
#else
            code = GUIUtility.systemCopyBuffer;
#endif
            return Import(code);
        }
    }
}
