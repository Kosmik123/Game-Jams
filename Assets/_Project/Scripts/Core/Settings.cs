using UnityEngine;

namespace PayWithPixels
{
    /// <summary>Ustawienia gracza (PlayerPrefs; w WebGL trafiaja do IndexedDB przegladarki).</summary>
    public static class Settings
    {
        private const string KeyVolume = "pwp_volume";
        private const string KeyMusic = "pwp_music";
        private const string KeySfx = "pwp_sfx";
        private const string KeyReduceFlicker = "pwp_reduce_flicker";
        private const string KeyShake = "pwp_shake";

        private static float volume = 0.8f;
        private static float music = 1f;
        private static float sfx = 1f;
        private static bool reduceFlicker;
        private static bool shake = true;
        private static bool loaded;

        public static float Volume
        {
            get { Load(); return volume; }
            set { volume = Mathf.Clamp01(value); AudioListener.volume = volume; Save(); }
        }

        /// <summary>Glosnosc muzyki 0..1 (mnoznik na glosnosc glowna).</summary>
        public static float MusicVolume
        {
            get { Load(); return music; }
            set { music = Mathf.Clamp01(value); Save(); Sfx.ApplyVolumes(); }
        }

        /// <summary>Glosnosc efektow 0..1.</summary>
        public static float SfxVolume
        {
            get { Load(); return sfx; }
            set { sfx = Mathf.Clamp01(value); Save(); Sfx.ApplyVolumes(); }
        }

        /// <summary>Wylacza inwersje palety (cena Migotanie) i jej odpowiedniki.</summary>
        public static bool ReduceFlicker
        {
            get { Load(); return reduceFlicker; }
            set { reduceFlicker = value; Save(); }
        }

        public static bool Shake
        {
            get { Load(); return shake; }
            set { shake = value; Save(); }
        }

        public static void Apply()
        {
            Load();
            AudioListener.volume = volume;
            Sfx.ApplyVolumes();
        }

        private static void Load()
        {
            if (loaded) return;
            loaded = true;
            try
            {
                volume = PlayerPrefs.GetFloat(KeyVolume, 0.8f);
                music = PlayerPrefs.GetFloat(KeyMusic, 1f);
                sfx = PlayerPrefs.GetFloat(KeySfx, 1f);
                reduceFlicker = PlayerPrefs.GetInt(KeyReduceFlicker, 0) == 1;
                shake = PlayerPrefs.GetInt(KeyShake, 1) == 1;
            }
            catch { /* brak PlayerPrefs - zostaja domyslne */ }
        }

        private static void Save()
        {
            try
            {
                PlayerPrefs.SetFloat(KeyVolume, volume);
                PlayerPrefs.SetFloat(KeyMusic, music);
                PlayerPrefs.SetFloat(KeySfx, sfx);
                PlayerPrefs.SetInt(KeyReduceFlicker, reduceFlicker ? 1 : 0);
                PlayerPrefs.SetInt(KeyShake, shake ? 1 : 0);
                PlayerPrefs.Save();
            }
            catch { }
        }
    }
}
