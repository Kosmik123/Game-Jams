using System;

/// <summary>
/// Numbers that drive creature movement. Fixed for now; later they will be computed from the genome
/// (flagella raise the speed and so on), so everything that moves a creature should read them from here.
/// </summary>
[Serializable]
public struct MovementStats
{
    /// <summary>World units per second, the same in every direction.</summary>
    public float moveSpeed;

    /// <summary>Degrees per second.</summary>
    public float rotationSpeed;

    public static MovementStats Default => new MovementStats
    {
        moveSpeed = 1f,
        rotationSpeed = 90f,
    };
}
