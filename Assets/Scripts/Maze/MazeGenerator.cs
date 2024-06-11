using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

[System.Flags]
public enum Passage
{
    Right = 1 << 0,
    Front = 1 << 1,
}

public struct Cell
{
    public Passage Passage { get; set; }
    public bool WasVisited { get; set; }
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

                }
                if (y > 0)
                {
                    AddEdge(new Vector2Int(x, y - 1), new Vector2Int(x, y));

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
        cells[currentCell.y, currentCell.x].WasVisited = true;

        var directions = GetRandomDirections();
        bool isDeadEnd = true;
        for (int i = 0; i < 4; i++)
        {
            var nextCell = currentCell + directions[i];
            if (IsValid(nextCell.x, nextCell.y) == false)
                continue;

            if (cells[nextCell.y, nextCell.x].WasVisited)
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
            cells[currentCell.y, x].Passage |= Passage.Right;
        }
        else if (currentCell.x == nextCell.x)
        {
            int y = Mathf.Min(currentCell.y, nextCell.y);
            cells[y, currentCell.x].Passage |= Passage.Front;
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
    public event System.Action OnMazeGenerated;

    [SerializeField]
    private Vector2Int size;

    private Maze maze;

    [SerializeField]
    private MazeBlock mazeBlockPrototype;
    [SerializeField]
    private float mazeBlockSize = 3.5f;

    private MazeBlock[,] mazeBlocks;
    public MazeBlock[,] MazeBlocks => mazeBlocks;

    [Header("Additional passages")]
    [SerializeField, Range(0, 1)]
    private float deadEndsConnectionRate;
    [SerializeField]
    private int additionalPassagesCount;

    [Header("Characters")]
    [SerializeField]
    private Transform player;
    [SerializeField]
    private Transform enemy;
    
    [Header("Vases")]
    [SerializeField]
    private GameObject[] vasePrefabs;
    [SerializeField]
    private Transform vasesContainer;

    [Header("Nav Mesh")]
    [SerializeField]
    private NavMeshSurface navMesh;


    private void Awake()
    {
        maze = new Maze(size);
        maze.Generate(Random.Range(0, size.x), Random.Range(0, size.y));
    }

    private IEnumerator Start()
    {
        additionalPassagesCount = Mathf.RoundToInt(maze.deadEnds.Count * deadEndsConnectionRate);
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
                var passage = maze.Cells[y, x].Passage;
                block.Configure(x, y, passage);

                if (passage.HasFlag(Passage.Front) == false)
                    cellsWithFrontWall.Add(new Vector2Int(x, y));
                if (passage.HasFlag(Passage.Right) == false)
                    cellsWithRightWall.Add(new Vector2Int(x, y));

                mazeBlocks[y, x] = block;
            }
            yield return null;
        }

        int xLast = size.x - 1;
        int yLast = size.y - 1;
        for (int i = 0; i < additionalPassagesCount; i++)
        {
            var list = maze.deadEnds;

            int randomIndex = Random.Range(0, list.Count);
            var cell = list[randomIndex];
            list.RemoveAt(randomIndex);

            if (cell.y == yLast && i % 2 == 0)
                continue;
            if (cell.x == xLast && i % 2 == 1)
                continue;

            mazeBlocks[cell.y, cell.x].DisableWall(i);
        }

        int xPlayerCoord = Random.Range(0, size.x);
        int yPlayerCoord = Random.Range(0, size.y);
        
        player.transform.position = new Vector3(
            xPlayerCoord * mazeBlockSize,
            0.5f,
            yPlayerCoord * mazeBlockSize);

        int xEnemyCoord = Mathf.RoundToInt(xPlayerCoord > size.x / 2 
            ? size.x / 4f 
            : 3 * size.x / 4f);
        int yEnemyCoord = Mathf.RoundToInt(yPlayerCoord > size.y / 2 
            ? size.y / 4f
            : 3 * size.y / 4f);

        enemy.position = new Vector3(
            xEnemyCoord * mazeBlockSize,
            0.0f,
            yEnemyCoord * mazeBlockSize);

        var mazeBlocksList = new List<MazeBlock>();
        for (int y = 0; y < size.y; y++)
            for (int x = 0; x < size.x; x++)
                mazeBlocksList.Add(mazeBlocks[y, x]);

        foreach (var vasePrototype in vasePrefabs)
        {
            int firstVaseLocationIndex = Random.Range(0, mazeBlocksList.Count);
            var firstVaseLocation = mazeBlocksList[firstVaseLocationIndex];
            mazeBlocksList.RemoveAt(firstVaseLocationIndex);

            int secondVaseLocationIndex = Random.Range(0, mazeBlocksList.Count);
            var secondVaseLocation = mazeBlocksList[secondVaseLocationIndex];
            mazeBlocksList.RemoveAt(secondVaseLocationIndex);

            var vase1 = Instantiate(vasePrototype, firstVaseLocation.GetVasePosition(),
                Quaternion.AngleAxis(Random.value * 360, Vector3.up), vasesContainer);
            vase1.name = vasePrototype.name + " (1)";

            var vase2 = Instantiate(vasePrototype, secondVaseLocation.GetVasePosition(),
                Quaternion.AngleAxis(Random.value * 360, Vector3.up), vasesContainer);
            vase2.name = vasePrototype.name + " (2)";
        }

        navMesh.BuildNavMesh();
        OnMazeGenerated?.Invoke();
    }

    public MazeBlock GetRandomMazeBlock()
    {
        int x = Random.Range(0, size.x);
        int y = Random.Range(0, size.y);
        return mazeBlocks[y, x];
    }
}
