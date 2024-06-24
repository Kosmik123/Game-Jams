using System.Collections.Generic;
using UnityEngine;

public class InfiniteMaze : MonoBehaviour
{
    public readonly Directions[] possibleStartingDirections =
    {
        Directions.East | Directions.North | Directions.West,
        Directions.North | Directions.West | Directions.South,
        Directions.West | Directions.South | Directions.East,
        Directions.South | Directions.East | Directions.North,
        Directions.East | Directions.North | Directions.West | Directions.South,
    };

    [SerializeField]
    private float generationRadius = 5;

    [SerializeField, Range(0, 1)]
    private float loopProbability;

    [SerializeField]
    private bool drawGizmos;

    private readonly Dictionary<Vector2Int, MazeCell> maze = new Dictionary<Vector2Int, MazeCell>();
    public IReadOnlyDictionary<Vector2Int, MazeCell> MazeCells => maze;

    private Vector2Int center = Vector2Int.zero;
    private Vector2Int previousCenter = Vector2Int.zero;

    private Vector2Int currentlyProcessedCoord;

    private GizmosDrawer gizmosDrawer = new GizmosDrawer();

    private List<Vector2Int> 
        cellsToProcessInThisStep = new List<Vector2Int>(), 
        cellsToProcessInNextStep = new List<Vector2Int>();

    private Stack<ProcessedCell> cellsPath = new Stack<ProcessedCell>();

    private void Start()
    {
        Vector2Int startingCoord = Vector2Int.zero;
        maze.Add(startingCoord, new MazeCell());
        CreatePathFromCell(startingCoord);
    }

    private void CreatePathFromCell(Vector2Int startingCoord)
    {
        cellsPath.Push(new ProcessedCell(startingCoord));
        while (cellsPath.Count > 0)
        {
            var processedCell = cellsPath.Pop();
            if (processedCell.checkedDirections == Directions.All)
                continue;

            var coord = processedCell.coord;
            float sqrRadius = generationRadius * generationRadius;
            if ((coord - center).sqrMagnitude > sqrRadius)
            {
                cellsToProcessInNextStep.Add(coord);
                continue;
            }

            var pathDirection = processedCell.checkedDirections.Inversed().GetRandomDirection();
            processedCell.AddDirection(pathDirection);
            cellsPath.Push(processedCell);

            var cell = maze[coord];
            var neighbourCoord = coord + pathDirection.ToVector();
            if (maze.TryGetValue(neighbourCoord, out var neighbourCell))
            {
                if (Random.value < loopProbability)
                {
                    ConnectCells(cell, pathDirection, neighbourCell);
                }
            }
            else
            {
                neighbourCell = new MazeCell();
                var neighbourDirection = ConnectCells(cell, pathDirection, neighbourCell);
                maze.Add(neighbourCoord, neighbourCell);
                cellsPath.Push(new ProcessedCell(neighbourCoord, neighbourDirection));
            }
        }
    }

    private static SingleDirection ConnectCells(MazeCell cell, SingleDirection pathDirection, MazeCell target)
    {
        cell.passageDirections = cell.passageDirections.With(pathDirection);
        var opposite = pathDirection.Opposite();
        target.passageDirections = target.passageDirections.With(opposite);
        return opposite;
    }

    private void GenerationStep()
    {
        (cellsToProcessInThisStep, cellsToProcessInNextStep) = (cellsToProcessInNextStep, cellsToProcessInThisStep);
        cellsToProcessInNextStep.Clear();

        while (cellsToProcessInThisStep.Count > 0)
        {
            var pathStartIndex = Random.Range(0, cellsToProcessInThisStep.Count);
            var pathStart = cellsToProcessInThisStep[pathStartIndex];
            cellsToProcessInThisStep.RemoveAt(pathStartIndex);

            float sqrRadius = generationRadius * generationRadius;
            if ((pathStart - center).sqrMagnitude > sqrRadius)
            {
                cellsToProcessInNextStep.Add(pathStart);
                continue;
            }
            CreatePathFromCell(pathStart);
        }
    }

    private void Update()
    {
        center = Vector2Int.RoundToInt(new Vector2(transform.position.x, transform.position.y));
        if (previousCenter != center)
        {
            GenerationStep();
            previousCenter = center;
        }
    }

    private void OnDrawGizmos()
    {
        if (drawGizmos && maze.Count > 0)
            gizmosDrawer.DrawGizmos(maze);
    }

    internal class GizmosDrawer
    {
        private readonly Queue<Vector2Int> cellsToCheck = new Queue<Vector2Int>();

        public void DrawGizmos(IReadOnlyDictionary<Vector2Int, MazeCell> cells)
        {
            Gizmos.color = Color.red;
            foreach (var pair in cells)
            {
                DrawCellGizmos(pair.Key, pair.Value);
            }
        }

        private void DrawCellGizmos(Vector2Int coord, MazeCell cell)
        {
            foreach (var direction in cell.passageDirections.ToVectors())
            {
                var neighbourCoord = coord + direction;
                var floatCoord = (Vector2)coord;
                Gizmos.DrawLine(floatCoord, (neighbourCoord + floatCoord) / 2f);
                Gizmos.DrawSphere(floatCoord, 0.1f);
            }
        }
    }
}

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

public class MazeCell
{
    public Directions passageDirections;
}

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
