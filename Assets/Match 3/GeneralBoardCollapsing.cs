using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Bipolar.Match3
{
    [RequireComponent(typeof(GeneralBoard))]
    public class GeneralBoardCollapsing : BoardCollapsing<GeneralBoard>
    {
        public override event System.Action OnPiecesColapsed;

        [SerializeField]
        private Tilemap[] directionsTilemaps;

        private GeneralBoardPiecesMovementManager piecesMovementManager;

        private readonly Dictionary<Vector2Int, Vector2Int> directions = new Dictionary<Vector2Int, Vector2Int>();

        private void Awake()
        {
            CreateCollapseDirections();
        }

        private void CreateCollapseDirections()
        {
            var tilemap = directionsTilemaps[0];
        }

        public void Init (GeneralBoardPiecesMovementManager movementManager)
        {
            piecesMovementManager = movementManager;
        }

        public override void Collapse()
        {
            bool collapsed = false;
            foreach (var line in Board.Lines)
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

        private bool TryGetTile(Vector2Int coord, Tilemap tilemap, out GeneralBoardTile tile)
        {
            tile = tilemap.GetTile<GeneralBoardTile>((Vector3Int)coord);
            return tile != null;
        }


        private int CollapseTokensInLine(GeneralBoard.CoordsLine line)
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

        private void RefillLine(GeneralBoard.CoordsLine line, int count)
        {
            var startCoord = line.Coords[0];
            var creatingDirection = -Board.GetRealDirection(startCoord);
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

        private void CallCollapseEvent()
        {
            piecesMovementManager.OnPiecesMovementStopped -= CallCollapseEvent;
            OnPiecesColapsed?.Invoke();
        }
    }
}
