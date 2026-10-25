using UnityEngine;
using ZeroHour.Tower;

namespace ZeroHour
{
    public enum GameVersionType
    {
        Jam,
        PostJam,
    }

    // Version picked in the VersionSelect scene. Starting Game directly in the editor plays post-jam.
    public static class GameVersion
    {
        public static GameVersionType Selected { get; set; } = GameVersionType.PostJam;

        public static TowerRuleset CreateRuleset()
        {
            return Selected == GameVersionType.Jam ? new JamRuleset() : (TowerRuleset)new PostJamRuleset();
        }

        // Statics survive play sessions when domain reload is disabled.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Selected = GameVersionType.PostJam;
        }
    }
}
