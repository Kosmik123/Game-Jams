public class MazeCell
{
    public Directions passageDirections;

    public void AddPassage(SingleDirection direction)
    {
        passageDirections = passageDirections.With(direction);
    }

    public static SingleDirection ConnectCells(MazeCell cell, SingleDirection pathDirection, MazeCell target)
    {
        cell.AddPassage(pathDirection);
        var opposite = pathDirection.Opposite();
        target.AddPassage(opposite);
        return opposite;
    }
}
