using System.Collections.Generic;
using UnityEngine;

[System.Flags]
public enum Directions
{
    None = 0,
    East = 1 << (SingleDirection.East - 1),
    North = 1 << (SingleDirection.North - 1),
    West = 1 << (SingleDirection.West - 1),
    South = 1 << (SingleDirection.South - 1),
    All = East | North | West | South
}

public enum SingleDirection
{
    None = 0,
    East = 1,
    North = 2,
    West = 3,
    South = 4,
}

public static class DirectionExtensions
{
    private static readonly Vector2Int[] directionVectors = { Vector2Int.right, Vector2Int.up, Vector2Int.left, Vector2Int.down };

    public const int AllDirectionsCount = 4;

    public static Directions Inversed(this Directions directions) => (Directions)(0b1111 ^ (int)directions);

    public static SingleDirection Opposite(this SingleDirection direction) => direction switch
    {
        SingleDirection.East => SingleDirection.West,
        SingleDirection.North => SingleDirection.South,
        SingleDirection.West => SingleDirection.East,
        SingleDirection.South => SingleDirection.North,
        _ => SingleDirection.None,
    };


    public static Vector2Int ToVector(this SingleDirection direction)
    {
        if (direction == SingleDirection.None)
            return Vector2Int.zero;

        return directionVectors[(int)direction - 1];
    }

    public static IEnumerable<Vector2Int> ToVectors(this Directions directions)
    {
        for (int i = 0; i < AllDirectionsCount; i++)
            if (directions.HasFlag((Directions)(1 << i)))
                yield return directionVectors[i];
    }

    public static Directions ToDirections(this SingleDirection direction)
    {
        if (direction == SingleDirection.None)
            return Directions.None;

        return (Directions)(1 << ((int)direction - 1));
    }

    public static SingleDirection ToSingleDirection(this Directions directions) => directions switch
    {
        Directions.East => SingleDirection.East,
        Directions.North => SingleDirection.North,
        Directions.West => SingleDirection.West,
        Directions.South => SingleDirection.South,
        _ => SingleDirection.None,
    };

    public static SingleDirection GetRandomDirection(this Directions directions)
    {
        int count = directions.Count();
        if (count == 0)
            return SingleDirection.None;

        int randomIndex = Random.Range(0, count);
        for (int i = 0; i < AllDirectionsCount; i++)
        {
            if (directions.HasFlag((Directions)(1 << i)))
            {
                if (randomIndex == 0)
                    return (SingleDirection)(i + 1);

                randomIndex--;
            }
        }
        return SingleDirection.None;
    }

    public static int Count(this Directions directions)
    {
        int count = 0;
        for (int i = 0; i < AllDirectionsCount; i++)
            if (directions.HasFlag((Directions)(1 << i)))
                count++;

        return count;
    }

    public static Directions Without(this Directions directions, SingleDirection without)
    {
        return directions.Without(without.ToDirections());
    }

    public static Directions With(this Directions directions, SingleDirection with)
    {
        return directions.With(with.ToDirections());
    }

    public static Directions Without(this Directions directions, Directions without)
    {
        return (Directions)((int)directions & ~(int)without);
    }

    public static Directions With(this Directions directions, Directions with)
    {
        return (Directions)((int)directions | (int)with);
    }
}
