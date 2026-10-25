using UnityEngine;

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

        public abstract CityStats CreateEconomy();

        public abstract TowerSway CreateSway();

        // Camera stays still and the tower sinks, instead of the camera following the tower up.
        public abstract bool FixedCamera { get; }

        // Touch moves the platform by the finger's movement instead of jumping under the finger
        // (the mouse still places it under the cursor).
        public abstract bool RelativeControl { get; }

        // World units of platform movement per world unit of finger movement (relative control).
        public abstract float DragSensitivity { get; }

        // PlayerPrefs key of the best score, or null when records aren't kept.
        public abstract string BestScoreKey { get; }

        public abstract string PopulationLabel { get; }

        // A perfect landing places the floor exactly on top of the previous one.
        public abstract bool SnapPerfectLandings { get; }

        public abstract bool ShowPerfectPopup { get; }

        public abstract bool ShowTutorial { get; }

        // "Menu" on the end screen; the jam version has no way back (reload the page).
        public abstract bool ShowMenuButton { get; }
    }

    // Frozen rules of the build shipped at the end of the jam (39a94ec).
    public class JamRuleset : TowerRuleset
    {
        public override string Name => "Jam";
        public override float RoundDuration => 40f;
        public override float FallSpeed(int towerHeight) => 3f;
        public override float SpawnInterval(int towerHeight) => 0.7f;
        public override CityStats CreateEconomy() => new CityStats();
        public override TowerSway CreateSway() => new JamTowerSway();
        public override bool FixedCamera => false;
        public override bool RelativeControl => false;
        public override string BestScoreKey => null;
        public override string PopulationLabel => "Pop";
        public override bool SnapPerfectLandings => false;
        public override float DragSensitivity => 0f;
        public override bool ShowPerfectPopup => false;
        public override bool ShowTutorial => false;
        public override bool ShowMenuButton => false;
    }

    public class PostJamRuleset : TowerRuleset
    {
        // Difficulty grows with every floor in the tower.
        private const float BaseFallSpeed = 3f;
        private const float FallSpeedPerFloor = 0.15f;
        private const float MaxFallSpeed = 8f;
        private const float BaseSpawnInterval = 0.7f;
        private const float SpawnIntervalPerFloor = 0.01f;
        private const float MinSpawnInterval = 0.4f;

        public override string Name => "Post-jam";
        public override float RoundDuration => 40f;
        public override float FallSpeed(int towerHeight) =>
            Mathf.Min(BaseFallSpeed + FallSpeedPerFloor * towerHeight, MaxFallSpeed);
        public override float SpawnInterval(int towerHeight) =>
            Mathf.Max(BaseSpawnInterval - SpawnIntervalPerFloor * towerHeight, MinSpawnInterval);
        public override CityStats CreateEconomy() => new PostJamCityStats();
        public override TowerSway CreateSway() => new PostJamTowerSway();
        public override bool FixedCamera => true;
        public override bool RelativeControl => true;
        public override string BestScoreKey => "BestScore_PostJam";
        public override string PopulationLabel => "Popul";
        public override bool SnapPerfectLandings => true;
        public override float DragSensitivity => 1f;
        public override bool ShowPerfectPopup => true;
        public override bool ShowTutorial => true;
        public override bool ShowMenuButton => true;
    }
}
