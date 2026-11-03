using UnityEngine;

/// <summary>
/// Balance numbers for creatures. Create an asset at Resources/CreatureConfig to override the defaults.
/// </summary>
[CreateAssetMenu(menuName = "Creature/Creature Config", fileName = "CreatureConfig")]
public class CreatureConfig : ScriptableObject
{
    [Tooltip("DNA the player has before eating anything (placeholder until the action mode feeds it).")]
    public int startingDna = 100;

    [Header("DNA costs")]
    [Min(0)] public int bodyCost = 5;
    [Min(0)] public int flagellumCost = 10;
    [Min(0)] public int spikesCost = 10;
    [Min(0)] public int armorCost = 15;

    private static CreatureConfig defaultInstance;

    /// <summary>Asset from Resources/CreatureConfig, or built-in defaults if there is none.</summary>
    public static CreatureConfig Default
    {
        get
        {
            if (defaultInstance == null)
            {
                defaultInstance = Resources.Load<CreatureConfig>("CreatureConfig");
                if (defaultInstance == null)
                {
                    defaultInstance = CreateInstance<CreatureConfig>();
                }
            }
            return defaultInstance;
        }
    }

    /// <summary>DNA cost of a module itself, not counting the body block under it.</summary>
    public int CostOf(ModuleType module)
    {
        switch (module)
        {
            case ModuleType.Flagellum: return flagellumCost;
            case ModuleType.Spikes: return spikesCost;
            case ModuleType.Armor: return armorCost;
            default: return 0;
        }
    }
}
