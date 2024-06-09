using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Flags]
public enum Passage
{
    Right = 1 << 0,
    Front = 1 << 1,
}

public struct Cell
{
    public Passage passage;
    public bool wasVisited;
}

public struct Edge
{
    public Vector2Int cell1;
    public Vector2Int cell2;

    public Edge(Vector2Int cell1, Vector2Int cell2)
    {
        this.cell1 = cell1;
        this.cell2 = cell2;
    }
}

public class Maze
{
    public static readonly Vector2Int[] possibleDirections =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.right,
        Vector2Int.left
    };

    private readonly Vector2Int size;

    private readonly Cell[,] cells;
    public Cell[,] Cells => cells;

    private readonly List<Edge> edges = new List<Edge>();

    public readonly List<Vector2Int> deadEnds = new List<Vector2Int>();

    public Maze(Vector2Int size)
    {
        this.size = size;
        cells = new Cell[size.y, size.x];

        int xLast = size.x - 1;
        int yLast = size.y - 1;
        for (int y = 0; y < size.y; y++)
        {
            for (int x = 0; x < size.x; x++)
            {
                if (x > 0)
                {
                    AddEdge(new Vector2Int(x - 1, y), new Vector2Int(x, y));
                    if (x == xLast)
                        cells[y, x].passage |= Passage.Right;
                }
                if (y > 0)
                {
                    AddEdge(new Vector2Int(x, y - 1), new Vector2Int(x, y));
                    if (y == yLast)
                        cells[y, x].passage |= Passage.Front;
                }
            }
        }
    }

    private void AddEdge(Vector2Int cellCoord1, Vector2Int cellCoord2)
    {
        var edge = new Edge(cellCoord1, cellCoord2);
        edges.Add(edge);
    }

    public void Generate(int xStart, int yStart)
    {
        var path = new List<Vector2Int>();
        VisitCell(new Vector2Int(xStart, yStart), path);
    }

    private void VisitCell(Vector2Int currentCell, List<Vector2Int> path)
    {
        path.Add(currentCell);
        cells[currentCell.y, currentCell.x].wasVisited = true;

        var directions = GetRandomDirections();
        bool isDeadEnd = true;
        for (int i = 0; i < 4; i++)
        {
            var nextCell = currentCell + directions[i];
            if (IsValid(nextCell.x, nextCell.y) == false)
                continue;

            if (cells[nextCell.y, nextCell.x].wasVisited)
                continue;

            isDeadEnd = false;
            CreatePassage(currentCell, nextCell);
            VisitCell(nextCell, path);
        }
        if (isDeadEnd)
            deadEnds.Add(currentCell);


        path.Remove(currentCell);
    }

    private void CreatePassage(Vector2Int currentCell, Vector2Int nextCell)
    {
        if (currentCell.y == nextCell.y)
        {
            int x = Mathf.Min(currentCell.x, nextCell.x);
            cells[currentCell.y, x].passage |= Passage.Right;
        }
        else if (currentCell.x == nextCell.x)
        {
            int y = Mathf.Min(currentCell.y, nextCell.y);
            cells[y, currentCell.x].passage |= Passage.Front;
        }
    }

    public bool IsValid(int x, int y)
    {
        return x >= 0 && y >= 0 && x < size.x && y < size.y;
    }

    private List<Vector2Int> GetRandomDirections()
    {
        var randomOrder = new List<Vector2Int>();
        var sorted = new List<Vector2Int>(possibleDirections);
        for (int i = 0; i < 4; i++)
        {
            int randomIndex = Random.Range(0, sorted.Count);
            var randomDirection = sorted[randomIndex];
            sorted.RemoveAt(randomIndex);
            randomOrder.Add(randomDirection);
        }
        return randomOrder;
    }
}

public class MazeGenerator : MonoBehaviour
{
    [SerializeField]
    private Vector2Int size;
    private Maze maze;
    
    [SerializeField]
    private Vector2Int startCoord;
    
    [SerializeField]
    private MazeBlock mazeBlockPrototype;
    [SerializeField]
    private float mazeBlockSize = 3.5f;

    private MazeBlock[,] mazeBlocks;

    [SerializeField]
    private int additionalPassagesCount;

    private void Awake()
    {
        maze = new Maze(size);
        maze.Generate(startCoord.x, startCoord.y);
    }

    private IEnumerator Start()
    {
	additionalPassagesCount = maze.deadEnds.Count / 2 + 1;
        var cellsWithFrontWall = new List<Vector2Int>();
        var cellsWithRightWall = new List<Vector2Int>();

        mazeBlocks = new MazeBlock[size.y, size.x];
        for (int y = 0; y < size.y; y++)
        {
            for (int x = 0; x < size.x; x++)
            {
                var worldPosition = new Vector3(
                    mazeBlockSize * x,
                    0,
                    mazeBlockSize * y);

                var block = Instantiate(mazeBlockPrototype, transform);
                block.name = $"{mazeBlockPrototype.name} ({x}, {y})";
                block.transform.localPosition = worldPosition;
                var passage = maze.Cells[y, x].passage;
                block.Configure(passage);

                if (passage.HasFlag(Passage.Front) == false)
                    cellsWithFrontWall.Add(new Vector2Int(x, y));
                if (passage.HasFlag(Passage.Right) == false)
                    cellsWithRightWall.Add(new Vector2Int(x, y));

                mazeBlocks[y, x] = block;
            }
            yield return null;
        }

        for (int i = 0; i < additionalPassagesCount; i++) 
        {
            var list = maze.deadEnds;

            int randomIndex = Random.Range(0, list.Count);
            var cell = list[randomIndex];
            list.RemoveAt(randomIndex);

            mazeBlocks[cell.y, cell.x].DisableWall(i);
        }
    }
}
