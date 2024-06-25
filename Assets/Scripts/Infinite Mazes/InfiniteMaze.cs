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

    private readonly Dictionary<Vector2Int, MazeCell> mazeCells = new Dictionary<Vector2Int, MazeCell>();
    public IReadOnlyDictionary<Vector2Int, MazeCell> MazeCells => mazeCells;

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
        mazeCells.Add(startingCoord, new MazeCell());
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

            var cell = mazeCells[coord];
            var neighbourCoord = coord + pathDirection.ToVector();
            if (mazeCells.TryGetValue(neighbourCoord, out var neighbourCell))
            {
                if (Random.value < loopProbability)
                {
                    MazeCell.ConnectCells(cell, pathDirection, neighbourCell);
                }
            }
            else
            {
                neighbourCell = new MazeCell();
                var neighbourDirection = MazeCell.ConnectCells(cell, pathDirection, neighbourCell);
                mazeCells.Add(neighbourCoord, neighbourCell);
                cellsPath.Push(new ProcessedCell(neighbourCoord, neighbourDirection));
            }
        }
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
        gizmosDrawer.DrawGizmos(this);
    }

    internal class GizmosDrawer
    {
        private readonly Queue<Vector2Int> cellsToCheck = new Queue<Vector2Int>();

        public void DrawGizmos(InfiniteMaze maze)
        {
            if (maze.drawGizmos && maze.mazeCells.Count > 0)
            {
                Gizmos.color = Color.red;
                foreach (var pair in maze.mazeCells)
                {
                    MazeGizmosUtility.DrawCellGizmos(pair.Key, pair.Value);
                }
            }
        }

    }
}

public static class MazeGizmosUtility
{
    public static void DrawCellGizmos(Vector2Int coord, MazeCell cell)
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
