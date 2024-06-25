using System.Collections.Generic;
using UnityEngine;

public class EndlessMaze : MonoBehaviour
{
    public struct ChunkIndex
    {
        public int x;
        public int y;

        public ChunkIndex(int x, int y) : this()
        {
            this.x = x;
            this.y = y;
        }

        public static ChunkIndex Zero = new ChunkIndex(0, 0);

        public static ChunkIndex operator +(ChunkIndex index, Vector2Int direction)
        {
            return new ChunkIndex(index.x + direction.x, index.y + direction.y);
        }

        public static bool operator ==(ChunkIndex lhs, ChunkIndex rhs) => lhs.Equals(rhs);
        public static bool operator !=(ChunkIndex lhs, ChunkIndex rhs) => !lhs.Equals(rhs);
    }

    [SerializeField]
    private uint chunkSize = 10;

    [SerializeField]
    private uint generationAreaExtent = 2;

    [SerializeField, Range(0, 1)]
    private float loopProbability = 0.1f;
    
    [SerializeField, Range(0, 1)]
    private float betweenChunksLoopProbability = 0.2f;

    private readonly Dictionary<ChunkIndex, MazeChunk> mazeChunks = new Dictionary<ChunkIndex, MazeChunk>();

    private ChunkIndex observerChunkIndex;
    private ChunkIndex previousObserverIndex;

    private void Start()
    {
        GenerateChunk(ChunkIndex.Zero);
        GenerateAroundObserver();
    }

    private void GenerateAroundObserver()
    {
        Vector2Int bottomLeft = new Vector2Int(
            Mathf.FloorToInt(observerChunkIndex.x - generationAreaExtent),
            Mathf.FloorToInt(observerChunkIndex.y - generationAreaExtent));

        Vector2Int topRight = new Vector2Int(
            Mathf.CeilToInt(observerChunkIndex.x + generationAreaExtent),
            Mathf.CeilToInt(observerChunkIndex.y + generationAreaExtent));

        for (int y = bottomLeft.y; y <= topRight.y; y++)
        {
            for (int x = bottomLeft.x; x <= topRight.x; x++)
            {
                var index = new ChunkIndex(x, y);
                GenerateChunk(index);
            }
        }
    }

    private Stack<ProcessedCell> cellsPath = new Stack<ProcessedCell>();
    private void GenerateChunk(ChunkIndex chunkIndex)
    {
        Debug.Log($"Generating chunk ({chunkIndex.x}, {chunkIndex.y})");
        if (mazeChunks.TryGetValue(chunkIndex, out var chunk) == false)
        {
            var startingCoord = new Vector2Int(
                Random.Range(0, (int)chunkSize),
                Random.Range(0, (int)chunkSize));

            mazeChunks.Add(chunkIndex, chunk = new MazeChunk(startingCoord, chunkSize, SingleDirection.None));
        }

        if (chunk.IsGenerated)
            return;

        cellsPath.Clear();
        cellsPath.Push(new ProcessedCell(chunk.startingCoord, chunk.StartingCell.passageDirections));
        while (cellsPath.Count > 0)
        {
            var processedCell = cellsPath.Pop();
            if (processedCell.checkedDirections == Directions.All)
                continue;

            var coord = processedCell.coord;
            var pathDirection = processedCell.checkedDirections.Inversed().GetRandomDirection();
            processedCell.AddDirection(pathDirection);
            cellsPath.Push(processedCell);

            var cell = chunk.GetCell(coord);
            var directionVector = pathDirection.ToVector();
            var neighbourCoord = coord + directionVector;
            if (IsInsideChunk(neighbourCoord) == false)
            {
                var neighbouringChunkIndex = chunkIndex + directionVector;

                // zainicjuj istnienie nowego chunka jeœli nie istnieje. 
                if (mazeChunks.TryGetValue(neighbouringChunkIndex, out var neighbouringChunk) == false)
                {
                    var startingCoordInNeighbourChunk = new Vector2Int(
                        (neighbourCoord.x + (int)chunkSize) % (int)chunkSize,
                        (neighbourCoord.y + (int)chunkSize) % (int)chunkSize);
                    neighbouringChunk = new MazeChunk(startingCoordInNeighbourChunk, chunkSize, pathDirection.Opposite());
                    mazeChunks.Add(neighbouringChunkIndex, neighbouringChunk);
                    cell.AddPassage(pathDirection);
                }
                else if (neighbouringChunk.IsGenerated)
                // Jeœli istnieje to po³¹cz siê z nim albo nie
                {
                    if (Random.value < betweenChunksLoopProbability)
                    {
                        var connectedCoordInNeighbourChunk = new Vector2Int(
                            (neighbourCoord.x + (int)chunkSize) % (int)chunkSize,
                            (neighbourCoord.y + (int)chunkSize) % (int)chunkSize);

                        MazeCell.ConnectCells(cell, pathDirection, neighbouringChunk.GetCell(connectedCoordInNeighbourChunk));
                    }
                }

                continue;
            }

            var neighbourCell = chunk.GetCell(neighbourCoord);
            if (neighbourCell == null)
            {
                neighbourCell = new MazeCell();
                var neighbourDirection = MazeCell.ConnectCells(cell, pathDirection, neighbourCell);
                chunk.SetCell(neighbourCoord, neighbourCell);
                cellsPath.Push(new ProcessedCell(neighbourCoord, neighbourDirection));
            }
            else
            {
                if (Random.value < loopProbability)
                {
                    MazeCell.ConnectCells(cell, pathDirection, neighbourCell);
                }
            }
        }
        chunk.IsGenerated = true;
    }

    private bool IsInsideChunk(Vector2Int neighbourCoord)
    {
        return neighbourCoord.x >= 0 && neighbourCoord.x < chunkSize 
            && neighbourCoord.y >= 0 && neighbourCoord.y < chunkSize;
    }

    private void Update()
    {
        var position = transform.position;
        float positionOffset = (chunkSize - 1) / 2f;
        observerChunkIndex = new ChunkIndex(
            Mathf.RoundToInt((position.x - positionOffset) / chunkSize),
            Mathf.RoundToInt((position.y - positionOffset) / chunkSize));

        if (observerChunkIndex != previousObserverIndex)
        {
            GenerateAroundObserver();
            previousObserverIndex = observerChunkIndex;
        }
    }

    private readonly GizmosDrawer gizmosDrawer = new GizmosDrawer();
    private void OnDrawGizmos()
    {
        gizmosDrawer.DrawGizmos(this);
    }

    internal class GizmosDrawer
    {
        public void DrawGizmos(EndlessMaze maze)
        {
            foreach (var chunk in maze.mazeChunks)
            {
                DrawChunkGizmos(chunk.Key, chunk.Value);
            }
        }

        private void DrawChunkGizmos(ChunkIndex index, MazeChunk chunk)
        {
            if (chunk.IsGenerated == false)
                return;

            int height = chunk.cells.GetLength(1);
            int width = chunk.cells.GetLength(0);
            for (int j = 0; j < height; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    var cell = chunk.GetCell(new Vector2Int(i, j));
                    var drawPosition = new Vector2Int(
                        index.x * width + i,
                        index.y * height + j);

                    Gizmos.color = (cell == chunk.StartingCell) ? Color.red : Color.white;
                    MazeGizmosUtility.DrawCellGizmos(drawPosition, cell);
                }
            }
        }
    }
}

public class MazeChunk
{
    public readonly Vector2Int startingCoord = -Vector2Int.one;
    public readonly MazeCell[,] cells;
    public bool IsGenerated { get; set; }

    public MazeCell StartingCell => cells[startingCoord.y, startingCoord.x];

    public MazeChunk(Vector2Int startingCoord, uint size, SingleDirection cellEnterDirection)
    {
        this.startingCoord = startingCoord;
        cells = new MazeCell[size, size];
        var startCell = new MazeCell { passageDirections = cellEnterDirection.ToDirections() };
        cells[startingCoord.y, startingCoord.x] = startCell;
    }

    public MazeCell GetCell(Vector2Int coord)
    {
        return cells[coord.y, coord.x];
    }

    public void SetCell(Vector2Int coord, MazeCell cell)
    {
        cells[coord.y, coord.x] = cell;
    }
}
