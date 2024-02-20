using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Bipolar.Match3
{
    [RequireComponent(typeof(GeneralBoard))]
    public class LinearGeneralBoardCollapsing : BoardCollapsing<GeneralBoard>
    {
        public override event System.Action OnPiecesColapsed;

        private GeneralBoardPiecesMovementManager piecesMovementManager;

        private readonly Dictionary<Vector2Int, Vector2Int> directions = new Dictionary<Vector2Int, Vector2Int>();
        private readonly HashSet<Vector2Int> jumpTiles = new HashSet<Vector2Int>();

        private CoordsLine[] lines;
        public IReadOnlyList<CoordsLine> Lines => lines;

        private HashSet<Vector2Int> startingCoords = new HashSet<Vector2Int>();
        private HashSet<Vector2Int> endingCoords = new HashSet<Vector2Int>();

        private void Awake()
        {
             CreateCollapseDirections();
        }

        public void Init(GeneralBoardPiecesMovementManager movementManager)
        {
            piecesMovementManager = movementManager;
        }

        [ContextMenu("Refresh")]
        private void CreateCollapseDirections()
        {
            directions.Clear();
            jumpTiles.Clear();

            startingCoords = new HashSet<Vector2Int>();
            endingCoords = new HashSet<Vector2Int>();
            var notStartingCoords = new HashSet<Vector2Int>();
            var notEndingCoords = new HashSet<Vector2Int>();

            var tempTargetCoordsDict = new Dictionary<Vector2Int, Vector2Int>();
            var tempSourceCoordsDict = new Dictionary<Vector2Int, Vector2Int>();

            bool isBoardHexagonal = Board.Grid.cellLayout == GridLayout.CellLayout.Hexagon;
            for (int coordIndex = 0; coordIndex < Board.Coords.Count; coordIndex++)
            {
                var coord = Board.Coords[coordIndex];
                if (TryGetTile(coord, out var tile) == false)
                    continue;

                var direction = DirectionTile.GetTileDirection(coord, tile, isBoardHexagonal);
                directions.Add(coord, direction);

                startingCoords.Add(coord);
                if (direction == Vector2Int.zero)
                {
                    endingCoords.Add(coord);
                    continue;
                }

                var targetCoord = coord + direction;
                if (Board.Coords.Contains(targetCoord) == false)
                {
                    endingCoords.Add(coord);
                    continue;
                }

                notEndingCoords.Add(coord);
                endingCoords.Add(targetCoord);
                notStartingCoords.Add(targetCoord);

                if (tempSourceCoordsDict.ContainsKey(targetCoord) == false)
                    tempSourceCoordsDict.Add(targetCoord, coord);
            }

            startingCoords.ExceptWith(notStartingCoords);
            endingCoords.ExceptWith(notEndingCoords);

            foreach (var kvp in tempSourceCoordsDict)
                tempTargetCoordsDict[kvp.Value] = kvp.Key;

            lines = new CoordsLine[startingCoords.Count];
            int lineIndex = 0;
            foreach (var coord in startingCoords)
            {
                lines[lineIndex] = CreateCoordsLine(coord, tempTargetCoordsDict);
                lineIndex++;
            }
            Debug.Log(lines.Length);
        }

        private CoordsLine CreateCoordsLine(Vector2Int startingCoord, IReadOnlyDictionary<Vector2Int, Vector2Int> targetCoordsDict)
        {
            var coordsList = new List<Vector2Int>();

            var coord = startingCoord;
            while (IndexOfCoordInBoard(coord) >= 0)
            {
                coordsList.Add(coord);
                if (targetCoordsDict.TryGetValue(coord, out var target) == false)
                    break;

                coord = target;
            }

            return new CoordsLine(coordsList);
        }

        //private int[] CreateCoordsIndexesArray(IReadOnlyCollection<Vector2Int> countCollection)
        //{
        //    int count = 0;
        //    if (countCollection.Count > 0)
        //        count = countCollection.Max() + 1;

        //    var array = new int[count];
        //    for (int i = 0; i < count; i++)
        //        array[i] = -1;

        //    return array;
        //}

        private bool TryGetTile(Vector2Int coord, out DirectionTile tile) => TryGetTile(coord, Board.Tilemap, out tile);

        public override void Collapse()
        {
            bool collapsed = false;
            foreach (var line in Lines)
            {
                int emptyCellsCount = CollapseTokensInLine(line);
                if (emptyCellsCount > 0)
                {
                    collapsed = true;
                    RefillLine(line, emptyCellsCount);
                }
            }

            if (collapsed)
                piecesMovementManager.OnPiecesMovementStopped += CallCollapseEvent;
        }

        private static bool TryGetTile(Vector2Int coord, Tilemap tilemap, out DirectionTile tile)
        {
            tile = tilemap.GetTile<DirectionTile>((Vector3Int)coord);
            return tile != null;
        }

        private int IndexOfCoordInBoard(Vector2Int coord)
        {
            for (int i = 0; i < Board.Coords.Count; i++)
                if (Board.Coords[i] == coord)
                    return i;

            return -1;
        }

        private int CollapseTokensInLine(CoordsLine line)
        {
            int nonExistingPiecesCount = 0;
            for (int index = line.Coords.Count - 1; index >= 0; index--)
            {
                var coord = line.Coords[index];
                var piece = Board.GetPiece(coord);
                if (piece == null || piece.IsCleared)
                {
                    nonExistingPiecesCount++;
                }
                else if (nonExistingPiecesCount > 0)
                {
                    var targetCoord = line.Coords[index + nonExistingPiecesCount];
                    Board[coord] = null;
                    Board[targetCoord] = piece;
                    piecesMovementManager.StartPieceMovement(piece, line, index, nonExistingPiecesCount);
                }
            }
            return nonExistingPiecesCount;
        }

        private void RefillLine(CoordsLine line, int count)
        {
            var startCoord = line.Coords[0];
            var creatingDirection = -GetRealDirection(startCoord);
            var firstCellPosition = Board.CoordToWorld(startCoord);
            for (int i = 0; i < count; i++)
            {
                var coord = line.Coords[i];
                var newPiece = CreatePiece(coord);
                var spawningPosition = firstCellPosition + (Vector3)(creatingDirection * (count - i));
                newPiece.transform.position = spawningPosition;
                piecesMovementManager.StartPieceMovement(newPiece, line, -1, i + 1);
            }
        }
        public Vector2Int GetDirection(Vector2Int coord) => directions[coord];

        public Vector2 GetRealDirection(Vector2Int coord)
        {
            var direction = GetDirection(coord);
            var nextCoord = coord + direction;

            var worldPos = Board.CoordToWorld(coord);
            var nextWorldPos = Board.CoordToWorld(nextCoord);

            return nextWorldPos - worldPos;
        }

        private void CallCollapseEvent()
        {
            piecesMovementManager.OnPiecesMovementStopped -= CallCollapseEvent;
            OnPiecesColapsed?.Invoke();
        }

        private void OnDrawGizmosSelected()
        {
            if (Lines != null && Board.Coords.Count > 0)
            {
                foreach (var line in Lines)
                {
                    for (int i = 0; i < line.Coords.Count; i++)
                    {
                        var coord = line.Coords[i];
                        if (i > 0)
                        {
                            var sourceCoord = line.Coords[i - 1];
                            GizmosDrawLineSegment(sourceCoord, coord);
                        }

                        if (i == 0)
                            GizmosDrawLineStart(coord);
                        
                        if (i == line.Coords.Count - 1)
                            GizmosDrawLineEnd(coord);
                        
                    }
                }
            }
        }

        private void GizmosDrawLineSegment(Vector2Int start, Vector2Int end)
        {
            var startPos = Board.CoordToWorld(start);
            var target = Board.CoordToWorld(end);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(startPos, target);
        }

        private void GizmosDrawLineStart(Vector2Int coord) => GizmosDrawLineTip(coord, Color.green, -0.1f);
        private void GizmosDrawLineEnd(Vector2Int coord) => GizmosDrawLineTip(coord, Color.red, 0.1f);
        private void GizmosDrawLineTip(Vector2Int coord, Color color, float offset)
        {
            if (TryGetTile(coord, out var tile))
            {
                Gizmos.color = color;
                Gizmos.DrawSphere(Board.CoordToWorld(coord) + (Vector3)(offset * (Vector2)tile.Direction), 0.1f);
            }
        }
    }
}

public class CoordsLine
{
    private Vector2Int[] coords;
    public IReadOnlyList<Vector2Int> Coords => coords;

    public CoordsLine(IEnumerable<Vector2Int> coords)
    {
        this.coords = coords.ToArray();
    }
}
