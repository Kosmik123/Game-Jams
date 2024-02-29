using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Bipolar.PuzzleBoard.General
{
    [RequireComponent(typeof(GeneralBoard))]
    public class LinearGeneralBoardCollapsing : BoardCollapsing<GeneralBoard>
    {
        public override event System.Action OnPiecesColapsed;

        [SerializeField]
        private LinearGeneralBoardPiecesMovementManager piecesMovementManager;

        private readonly Dictionary<Vector2Int, Vector2Int> directions = new Dictionary<Vector2Int, Vector2Int>();
        private readonly HashSet<Vector2Int> jumpTiles = new HashSet<Vector2Int>();

        private CoordsLine[] lines;
        public IReadOnlyList<CoordsLine> Lines => lines;

        public override bool IsCollapsing => piecesMovementManager.ArePiecesMoving;

        private void Awake()
        {
            CreateLines();
        }

        [ContextMenu("Refresh")]
        private void CreateLines()
        {
            directions.Clear();
            jumpTiles.Clear();

            var startingCoords = new HashSet<Vector2Int>();
            var endingCoords = new HashSet<Vector2Int>();
            var notStartingCoords = new HashSet<Vector2Int>();
            var notEndingCoords = new HashSet<Vector2Int>();

            var tempTargetCoordsDict = new Dictionary<Vector2Int, Vector2Int>();
            var tempSourceCoordsDict = new Dictionary<Vector2Int, Vector2Int>();

            bool isBoardHexagonal = Board.Grid.cellLayout == GridLayout.CellLayout.Hexagon;
            for (int coordIndex = 0; coordIndex < Board.Coords.Count; coordIndex++)
            {
                var coord = Board.Coords[coordIndex];
                HandleCoordCreation(coord, isBoardHexagonal, startingCoords, endingCoords,
                    notStartingCoords, notEndingCoords, tempSourceCoordsDict);
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
        }

        private void HandleCoordCreation(Vector2Int coord, bool isBoardHexagonal,
            HashSet<Vector2Int> startingCoords, HashSet<Vector2Int> endingCoords,
            HashSet<Vector2Int> notStartingCoords, HashSet<Vector2Int> notEndingCoords,
            Dictionary<Vector2Int, Vector2Int> targetToSourceDictionary)
        {
            if (TryGetTile(coord, out var tile) == false)
                return;

            var direction = DirectionTileHelper.GetTileDirection(coord, tile, isBoardHexagonal);
            directions.Add(coord, direction);

            startingCoords.Add(coord);
            if (direction == Vector2Int.zero)
            {
                endingCoords.Add(coord);
                return;
            }

            var targetCoord = coord + direction;
            if (tile.Jump)
            {
                bool hasJumpTarget = gameObject;
                while (TilemapContainsCoord(Board.ShapeTilemap, targetCoord))
                {
                    if (Board.Contains(targetCoord))
                    {
                        hasJumpTarget = true;
                        break;
                    }
                    targetCoord += DirectionTileHelper.GetTileDirection(targetCoord, tile, isBoardHexagonal);
                }

                if (hasJumpTarget == false)
                {
                    endingCoords.Add(coord);
                    return;
                }
            }
            else if (Board.Coords.Contains(targetCoord) == false)
            {
                endingCoords.Add(coord);
                return;
            }

            notEndingCoords.Add(coord);
            endingCoords.Add(targetCoord);
            notStartingCoords.Add(targetCoord);

            if (targetToSourceDictionary.ContainsKey(targetCoord) == false)
                targetToSourceDictionary.Add(targetCoord, coord);
        }

        public static bool TilemapContainsCoord(Tilemap tilemap, Vector2Int coord)
        {
            var bounds = tilemap.cellBounds;
            return coord.y >= bounds.yMin && coord.y < bounds.yMax && coord.x >= bounds.xMin && coord.x < bounds.xMax;
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

        private bool TryGetTile(Vector2Int coord, out DirectionTile tile) => DirectionTileHelper.TryGetTile(coord, Board.ShapeTilemap, out tile);

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
                piecesMovementManager.OnAllPiecesMovementStopped += CallCollapseEvent;
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
            piecesMovementManager.OnAllPiecesMovementStopped -= CallCollapseEvent;
            OnPiecesColapsed?.Invoke();
        }

        private void OnDrawGizmosSelected()
        {
            if (Lines != null && Board.Coords.Count > 0)
                foreach (var line in Lines)
                    GizmosDrawLine(line);

            void GizmosDrawLine(CoordsLine line)
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

            void GizmosDrawLineSegment(Vector2Int start, Vector2Int end)
            {
                var startPos = Board.CoordToWorld(start);
                var target = Board.CoordToWorld(end);
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(startPos, target);
            }

            void GizmosDrawLineStart(Vector2Int coord) => GizmosDrawLineTip(coord, Color.green, -0.1f);
            void GizmosDrawLineEnd(Vector2Int coord) => GizmosDrawLineTip(coord, Color.red, 0.1f);
            void GizmosDrawLineTip(Vector2Int coord, Color color, float offset)
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
        private readonly Vector2Int[] coords;
        public IReadOnlyList<Vector2Int> Coords => coords;

        public CoordsLine(IEnumerable<Vector2Int> coords)
        {
            this.coords = coords.ToArray();
        }
    }
}