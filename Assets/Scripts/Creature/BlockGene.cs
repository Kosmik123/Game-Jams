using System;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// One block of a creature: grid position, module on it and the side the module works on.
/// Immutable data only. Runtime state (current HP etc.) lives on the creature's block objects.
/// </summary>
[Serializable]
public struct BlockGene : IEquatable<BlockGene>
{
    public Vector2Int position;
    public ModuleType module;
    public Direction direction;

    public BlockGene(Vector2Int position, ModuleType module = ModuleType.None, Direction direction = Direction.Up)
    {
        this.position = position;
        this.module = module;
        this.direction = direction;
	}

    public BlockGene(int x, int y, ModuleType module = ModuleType.None, Direction direction = Direction.Up) : this(new Vector2Int(x, y), module, direction)
    { }

	public readonly bool Equals(BlockGene other)
    {
        return position == other.position && module == other.module && direction == other.direction;
    }

    public override readonly bool Equals(object obj)
    {
        return obj is BlockGene other && Equals(other);
    }

    public override readonly int GetHashCode()
    {
        return HashCode.Combine(position, module, direction);
    }
}
