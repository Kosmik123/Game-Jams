using UnityEngine;
using UnityEngine.EventSystems;

namespace Bipolar.Match3
{
    public class RectangularBoardCollapsing : BoardCollapsing<RectangularBoard>
    {
        public override event System.Action OnPiecesColapsed;

        protected RectangularBoardPiecesMovementManager piecesMovementManager;

        [SerializeField]
        private MoveDirection collapseDirection;
        public Vector2Int CollapseDirection
        {
            get
            {
                var dir = collapseDirection switch
                {
                    MoveDirection.Up => Vector3.up,
                    MoveDirection.Left => Vector3.left,
                    MoveDirection.Right => Vector3.right,
                    MoveDirection.Down => Vector3.down,
                    _ => Vector3.zero,
                };
                return Vector2Int.RoundToInt(Grid.Swizzle(Board.Grid.cellSwizzle, dir));
            }
        }

        public void Init(RectangularBoardPiecesMovementManager piecesMovementManager)
        {
            this.piecesMovementManager = piecesMovementManager;
        }

        public override void Collapse()
        {
            int iterationAxis = (CollapseDirection.x != 0) ? 1 : 0;
            bool colapsed = false;
            for (int lineIndex = 0; lineIndex < Board.Dimensions[iterationAxis]; lineIndex++)
            {
                int emptyCellsCount = CollapseTokensInLine(lineIndex, iterationAxis);
                if (emptyCellsCount > 0)
                {
                    colapsed = true;
                    RefillLine(lineIndex, emptyCellsCount, iterationAxis);
                }
            }

            if (colapsed)
                piecesMovementManager.OnPiecesMovementStopped += CallCollapseEvent;
        }

        private int CollapseTokensInLine(int lineIndex, int iterationAxis)
        {
            int collapseAxis = 1 - iterationAxis; // to samo
            int lineSize = Board.Dimensions[collapseAxis]; // to samo

            int startCellIndex = CollapseDirection[collapseAxis] > 0 ? -1 : 0; // odwrócony warunek
            int lineCollapseDirection = CollapseDirection[collapseAxis] == 0 ? 1 : -CollapseDirection[collapseAxis];

            int nonExistingPiecesCount = 0; // inna rzecz
            for (int i = 0; i < lineSize; i++) // troszkę inne
            {
                var coord = Vector2Int.zero; // to samo
                coord[iterationAxis] = lineIndex; // to samo
                coord[collapseAxis] = (startCellIndex + i * lineCollapseDirection + lineSize) % lineSize; // odwrócony znak

                // odtąd są inne czynnności
                var piece = Board.GetPiece(coord);
                if (piece == null || piece.IsCleared)
                {
                    nonExistingPiecesCount++;
                }
                else if (nonExistingPiecesCount > 0)
                {
                    var offsetToMove = CollapseDirection * nonExistingPiecesCount;
                    var targetCoord = coord + offsetToMove;
                    Board[coord] = null;
                    Board[targetCoord] = piece;
                    piecesMovementManager.StartPieceMovement(piece, targetCoord, 0.3f); // to samo
                }
            }

            return nonExistingPiecesCount;
        }

        private void CallCollapseEvent()
        {
            piecesMovementManager.OnPiecesMovementStopped -= CallCollapseEvent;
            OnPiecesColapsed?.Invoke();
        }

        private void RefillLine(int lineIndex, int count, int iterationAxis)
        {
            int collapseAxis = 1 - iterationAxis; // to samo
            int lineSize = Board.Dimensions[collapseAxis]; // to samo

            int startCellIndex = CollapseDirection[collapseAxis] < 0 ? -1 : 0; // odwrócony warunek
            var spawnOffset = -CollapseDirection * count; // inna rzecz

            int refillingDirection = CollapseDirection[collapseAxis] == 0 ? 1 : CollapseDirection[collapseAxis];

            for (int i = 0; i < count; i++)
            {
                var coord = Vector2Int.zero; // to samo
                coord[iterationAxis] = lineIndex; // to samo
                coord[collapseAxis] = (startCellIndex + i * refillingDirection + lineSize) % lineSize; // odwrócony znak

                // odtąd inne czynnności
                var newPiece = CreatePiece(coord);
                var spawnCoord = coord + spawnOffset;
                newPiece.transform.position = Board.CoordToWorld(spawnCoord);
                piecesMovementManager.StartPieceMovement(newPiece, coord, 0.3f); // to samo
            }
        }


    }
}
