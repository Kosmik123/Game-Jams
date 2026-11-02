using UnityEngine;

public enum Direction
{
    Up = 0,
    Right,
    Down,
    Left,
}

public static class DirectionExtensions
{
    public static Vector2Int ToVector(this Direction direction)
    {
        switch (direction)
        {
            case Direction.Up: return Vector2Int.up;
            case Direction.Right: return Vector2Int.right;
            case Direction.Down: return Vector2Int.down;
            default: return Vector2Int.left;
        }
    }

    public static Direction RotateClockwise(this Direction direction)
    {
        return (Direction)(((int)direction + 1) % 4);
    }

    public static Direction RotateCounterClockwise(this Direction direction)
    {
        return (Direction)(((int)direction + 3) % 4);
    }

    public static Direction Opposite(this Direction direction)
    {
        return (Direction)(((int)direction + 2) % 4);
    }

    /// <summary>Angle in degrees, counter-clockwise from Up (Up = 0, Left = 90).</summary>
    public static float ToAngle(this Direction direction)
    {
        return 90f * (4 - (int)direction) % 360f;
    }
}
