using UnityEngine;

namespace ZeroHour.Tower
{
    // Everything that differs between game versions. Shared code asks the ruleset
    // instead of checking which version is running.
    public abstract class TowerRuleset
    {
        public abstract string Name { get; }

        // Seconds on the clock when the round starts.
        public abstract float RoundDuration { get; }

        // The clock never shows more than this; extra time is lost.
        public abstract float MaxTime { get; }

        // Seconds added for a caught floor; towerHeight is the height after the catch.
        public abstract float FloorTimeBonus(int towerHeight, bool isPerfect);

        // Seconds taken for a collapse (a positive number).
        public abstract float CollapseTimePenalty { get; }

        // The timer turns red when it shows this many seconds or fewer.
        public abstract int TimerWarningSeconds { get; }

        // Time left after a bonus or a penalty, kept within 0..MaxTime.
        public float ApplyTimeChange(float timeLeft, float change) => Mathf.Clamp(timeLeft + change, 0f, MaxTime);

        // Units per second for a floor spawned when the tower has the given height.
        public abstract float FallSpeed(int towerHeight);

        // Seconds until the next floor, measured when the tower has the given height.
        public abstract float SpawnInterval(int towerHeight);

        public abstract CityStats CreateEconomy();

        public abstract TowerSway CreateSway();

        // Camera stays still and the tower sinks, instead of the camera following the tower up.
        public abstract bool FixedCamera { get; }

        // Screen height fraction (from the bottom) where the tower top is kept; the rest is sky for
        // catching floors.
        public abstract float TowerTopScreenFraction { get; }

        // The sky changes from day to night as the tower grows; off keeps the plain sky color.
        public virtual bool DynamicSky => false;

        // Golden floor: a rare, faster floor that pays a bonus. Off (zero chance) unless a version opts in.
        public virtual float GoldChance => 0f;
        public virtual int GoldMinHeight => 0;
        public virtual int GoldPity => 0;
        public virtual float GoldFallSpeedFactor => 1f;
        public virtual float GoldTimeBonus => 0f;
        public virtual float GoldInstabilityRelief => 0f;
        public virtual float GoldMoneyBonus => 0f;

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

        // At the end of the round the camera zooms out to show the whole building.
        public abstract bool ShowTowerShot { get; }

        // Screen shake on collapse, "+$" from the roof and squashy landings.
        public abstract bool ShowJuice { get; }

        // Floors are always 1 unit wide; their height and art (Resources/<folder>) depend on the version.
        public abstract float FloorHeight { get; }

        public abstract string FloorArtFolder { get; }

        // Sound effects, music and the mute button (the version select clicks always sound).
        public abstract bool PlaySounds { get; }

        public abstract bool ShowPerfectPopup { get; }

        public abstract bool ShowTutorial { get; }

        // "Menu" on the end screen, back to the version select.
        public abstract bool ShowMenuButton { get; }
    }

    // Frozen rules of the build shipped at the end of the jam (39a94ec).
    public class JamRuleset : TowerRuleset
    {
        public override string Name => "Jam";
        public override float RoundDuration => 40f;
        public override float MaxTime => RoundDuration;
        public override float FloorTimeBonus(int towerHeight, bool isPerfect) => 0f;
        public override float CollapseTimePenalty => 0f;
        public override int TimerWarningSeconds => 10;
        public override float FallSpeed(int towerHeight) => 3f;
        public override float SpawnInterval(int towerHeight) => 0.7f;
        public override CityStats CreateEconomy() => new CityStats();
        public override TowerSway CreateSway() => new JamTowerSway();
        public override bool FixedCamera => false;
        public override float TowerTopScreenFraction => 0.4f;
        public override bool RelativeControl => false;
        public override string BestScoreKey => null;
        public override string PopulationLabel => "Pop";
        public override bool SnapPerfectLandings => false;
        public override bool ShowTowerShot => false;
        public override bool ShowJuice => false;
        public override bool PlaySounds => false;
        public override float FloorHeight => 1f;
        public override string FloorArtFolder => "Floors";
        public override float DragSensitivity => 0f;
        public override bool ShowPerfectPopup => false;
        public override bool ShowTutorial => false;
        public override bool ShowMenuButton => true;
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

        // The clock starts low and the player earns time by building.
        private const float StartTime = 25f;
        private const float MaxRoundTime = 30f;
        private const float TimePerFloor = 1f;
        private const float TimeFalloffHeight = 40f;
        private const float MinTimeFactor = 0.3f;
        private const float PerfectTimeBonus = 1f;
        private const float CollapsePenalty = 4f;

        // Golden floor.
        private const float GoldSpawnChance = 0.05f;
        private const int GoldSpawnMinHeight = 3;
        private const int GoldSpawnPity = 25;
        private const float GoldSpeedFactor = 1.4f;
        private const float GoldBonusTime = 3f;
        private const float GoldBonusStability = 25f;
        private const float GoldBonusMoney = 25f;

        public override string Name => "Post-jam";
        public override float RoundDuration => StartTime;
        public override float MaxTime => MaxRoundTime;
        public override float CollapseTimePenalty => CollapsePenalty;
        public override int TimerWarningSeconds => 4;

        // The floor bonus shrinks as the tower grows; the perfect bonus doesn't.
        public override float FloorTimeBonus(int towerHeight, bool isPerfect) =>
            TimePerFloor * Mathf.Max(MinTimeFactor, 1f - towerHeight / TimeFalloffHeight) + (isPerfect ? PerfectTimeBonus : 0f);
        public override float FallSpeed(int towerHeight) =>
            Mathf.Min(BaseFallSpeed + FallSpeedPerFloor * towerHeight, MaxFallSpeed);
        public override float SpawnInterval(int towerHeight) =>
            Mathf.Max(BaseSpawnInterval - SpawnIntervalPerFloor * towerHeight, MinSpawnInterval);
        public override CityStats CreateEconomy() => new PostJamCityStats();
        public override TowerSway CreateSway() => new PostJamTowerSway();
        public override bool FixedCamera => true;
        public override float TowerTopScreenFraction => 0.25f;
        public override bool DynamicSky => true;
        public override float GoldChance => GoldSpawnChance;
        public override int GoldMinHeight => GoldSpawnMinHeight;
        public override int GoldPity => GoldSpawnPity;
        public override float GoldFallSpeedFactor => GoldSpeedFactor;
        public override float GoldTimeBonus => GoldBonusTime;
        public override float GoldInstabilityRelief => GoldBonusStability;
        public override float GoldMoneyBonus => GoldBonusMoney;
        public override bool RelativeControl => true;
        public override string BestScoreKey => "BestScore_PostJam";
        public override string PopulationLabel => "Popul";
        public override bool SnapPerfectLandings => true;
        public override bool ShowTowerShot => true;
        public override bool ShowJuice => true;
        public override bool PlaySounds => true;
        public override float FloorHeight => 0.75f;
        public override string FloorArtFolder => "FloorsWide";
        public override float DragSensitivity => 1f;
        public override bool ShowPerfectPopup => true;
        public override bool ShowTutorial => true;
        public override bool ShowMenuButton => true;
    }
}
