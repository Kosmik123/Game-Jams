using System.Collections.Generic;
using UnityEngine;

namespace PayWithPixels
{
    public enum Weapon { Pistol, Shotgun, Laser }

    public class Achievement
    {
        public string id, title, description, icon;
    }

    /// <summary>Postep miedzy runami: rekord, osiagniecia, odblokowane bronie (PlayerPrefs).</summary>
    public static class Meta
    {
        private const string KeyBest = "pwp_best_wave";
        private const string KeyWins = "pwp_wins";
        private const string KeyRuns = "pwp_runs";
        private const string KeyAch = "pwp_ach";
        private const string KeyWeapon = "pwp_weapon";
        private const string KeyTutorial = "pwp_tutorial";
        private const string KeyAscMax = "pwp_asc_max";
        private const string KeyAscSel = "pwp_asc_sel";
        private const string KeyBestScore = "pwp_best_score";

        public static readonly Achievement[] Achievements =
        {
            new Achievement { id = "boss1", title = "TOLL PAID", description = "DEFEAT THE TOLL KEEPER", icon = "ach_boss1" },
            new Achievement { id = "boss2", title = "BANKRUPT", description = "DEFEAT THE BANKER", icon = "ach_boss2" },
            new Achievement { id = "boss3", title = "SOLD OUT", description = "DEFEAT THE AUCTIONEER", icon = "ach_boss3" },
            new Achievement { id = "fog3", title = "BLIND FAITH", description = "HAVE 3 STACKS OF FOG AT ONCE", icon = "ach_fog3" },
            new Achievement { id = "debtfree", title = "DEBT FREE", description = "WIN A RUN OWING NOTHING", icon = "ach_debt_free" },
            new Achievement { id = "rich", title = "FILTHY RICH", description = "HOLD 100 COINS", icon = "ach_rich" },
            new Achievement { id = "untouchable", title = "UNTOUCHABLE", description = "CLEAR 3 WAVES IN A ROW WITHOUT A HIT", icon = "ach_shield" },
            new Achievement { id = "speed", title = "TIME IS MONEY", description = "WIN IN UNDER 12 MINUTES", icon = "ach_speed" },
            new Achievement { id = "fineprint", title = "READ THE FINE PRINT", description = "SEE A CONTRACT'S FINE PRINT KICK IN", icon = "ach_fineprint" },
            new Achievement { id = "signed", title = "SIGNED AND SEALED", description = "SIGN 3 CONTRACTS IN ONE RUN", icon = "ach_signed" },
            new Achievement { id = "survivor", title = "SURVIVOR", description = "REACH ZONE 3", icon = "ach_survivor" },
        };

        private static readonly HashSet<string> unlocked = new HashSet<string>();
        private static bool loaded;

        public static int BestWave { get { Load(); return best; } }
        public static int Wins { get { Load(); return wins; } }
        public static int Runs { get { Load(); return runs; } }
        private static int best, wins, runs, ascMax, ascSel, bestScore;

        public const int MaxAscension = 5;

        /// <summary>Najwyzszy odblokowany poziom Ascension (0 = brak, odblokowuje sie wygrana).</summary>
        public static int AscensionUnlocked { get { Load(); return ascMax; } }
        public static int BestScore { get { Load(); return bestScore; } }

        public static int Ascension
        {
            get { Load(); return Mathf.Min(ascSel, ascMax); }
            set { ascSel = Mathf.Clamp(value, 0, ascMax); Save(); }
        }

        public static bool TutorialDone
        {
            get { Load(); return tutorial; }
            set { tutorial = value; Save(); }
        }
        private static bool tutorial;

        public static Weapon Selected
        {
            get { Load(); return IsWeaponUnlocked(selected) ? selected : Weapon.Pistol; }
            set { selected = value; Save(); }
        }
        private static Weapon selected = Weapon.Pistol;

        public static bool Has(string id) { Load(); return unlocked.Contains(id); }

        /// <summary>Odblokowuje osiagniecie; zwraca true tylko za pierwszym razem.</summary>
        public static bool Unlock(string id)
        {
            Load();
            if (!unlocked.Add(id)) return false;
            Save();
            return true;
        }

        public static Achievement Find(string id)
        {
            foreach (var a in Achievements) if (a.id == id) return a;
            return null;
        }

        public static bool IsWeaponUnlocked(Weapon w)
        {
            Load();
            switch (w)
            {
                case Weapon.Shotgun: return unlocked.Contains("boss1");
                case Weapon.Laser: return unlocked.Contains("boss2");
                default: return true;
            }
        }

        public static string WeaponUnlockHint(Weapon w) => w == Weapon.Shotgun ? "BEAT THE TOLL KEEPER" : w == Weapon.Laser ? "BEAT THE BANKER" : "";

        public static void RecordRun(int waveReached, bool won, int ascension = 0, int score = 0)
        {
            Load();
            runs++;
            if (won)
            {
                wins++;
                ascMax = Mathf.Min(MaxAscension, Mathf.Max(ascMax, ascension + 1));
            }
            if (score > bestScore) bestScore = score;
            if (waveReached > best) best = waveReached;
            Save();
        }

        private static void Load()
        {
            if (loaded) return;
            loaded = true;
            try
            {
                best = PlayerPrefs.GetInt(KeyBest, 0);
                wins = PlayerPrefs.GetInt(KeyWins, 0);
                runs = PlayerPrefs.GetInt(KeyRuns, 0);
                tutorial = PlayerPrefs.GetInt(KeyTutorial, 0) == 1;
                ascMax = PlayerPrefs.GetInt(KeyAscMax, 0);
                ascSel = PlayerPrefs.GetInt(KeyAscSel, 0);
                bestScore = PlayerPrefs.GetInt(KeyBestScore, 0);
                selected = (Weapon)PlayerPrefs.GetInt(KeyWeapon, 0);
                foreach (var id in PlayerPrefs.GetString(KeyAch, "").Split(','))
                    if (!string.IsNullOrEmpty(id)) unlocked.Add(id);
            }
            catch { }
        }

        private static void Save()
        {
            try
            {
                PlayerPrefs.SetInt(KeyBest, best);
                PlayerPrefs.SetInt(KeyWins, wins);
                PlayerPrefs.SetInt(KeyRuns, runs);
                PlayerPrefs.SetInt(KeyTutorial, tutorial ? 1 : 0);
                PlayerPrefs.SetInt(KeyAscMax, ascMax);
                PlayerPrefs.SetInt(KeyAscSel, ascSel);
                PlayerPrefs.SetInt(KeyBestScore, bestScore);
                PlayerPrefs.SetInt(KeyWeapon, (int)selected);
                PlayerPrefs.SetString(KeyAch, string.Join(",", unlocked));
                PlayerPrefs.Save();
            }
            catch { }
        }
    }
}
