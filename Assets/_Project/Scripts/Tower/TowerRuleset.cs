namespace ZeroHour.Tower
{
    // Everything that differs between game versions. Shared code asks the ruleset
    // instead of checking which version is running.
    public abstract class TowerRuleset
    {
        public abstract string Name { get; }

        public abstract float RoundDuration { get; }

        // Units per second for a floor spawned when the tower has the given height.
        public abstract float FallSpeed(int towerHeight);

        // Seconds until the next floor, measured when the tower has the given height.
        public abstract float SpawnInterval(int towerHeight);

        public abstract bool ShowPerfectPopup { get; }

        public abstract bool ShowTutorial { get; }

        // "Menu" on the end screen; the jam version has no way back (reload the page).
        public abstract bool ShowMenuButton { get; }
    }

    // Frozen rules of the build shipped at the end of the jam (39a94ec).
    public class JamRuleset : TowerRuleset
    {
        public override string Name => "Jam";
        public override float RoundDuration => 90f;
        public override float FallSpeed(int towerHeight) => 3f;
        public override float SpawnInterval(int towerHeight) => 0.7f;
        public override bool ShowPerfectPopup => false;
        public override bool ShowTutorial => false;
        public override bool ShowMenuButton => false;
    }

    public class PostJamRuleset : TowerRuleset
    {
        public override string Name => "Post-jam";
        public override float RoundDuration => 40f;
        public override float FallSpeed(int towerHeight) => 3f;
        public override float SpawnInterval(int towerHeight) => 0.7f;
        public override bool ShowPerfectPopup => false;
        public override bool ShowTutorial => false;
        public override bool ShowMenuButton => true;
    }
}
