using UnityEngine;

public struct ProcessedCell
{
    public Vector2Int coord;
    public Directions checkedDirections;

    public ProcessedCell(Vector2Int coord, Directions checkedDirections = Directions.None)
    {
        this.coord = coord;
        this.checkedDirections = checkedDirections;
    }

    public ProcessedCell(Vector2Int coord, SingleDirection checkedDirection)
    {
        this.coord = coord;
        checkedDirections = checkedDirection.ToDirections();
    }

    public void AddDirection(SingleDirection direction)
    {
        checkedDirections = checkedDirections.With(direction);
    }

    public override string ToString() => $"{coord}: {checkedDirections}";
}
