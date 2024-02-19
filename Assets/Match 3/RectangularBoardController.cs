using UnityEngine;

namespace Bipolar.Match3
{
    [RequireComponent(typeof(RectangularBoard), typeof(RectangularBoardCollapsing))]
    public class RectangularBoardController : BoardController<RectangularBoard, RectangularBoardCollapsing>
    {
        public override event System.Action OnPiecesColapsed;
        public override event PiecesSwapEventHandler OnPiecesSwapped;

        [SerializeField]
        private RectangularBoardPiecesMovementManager piecesMovementManager;

        public override bool ArePiecesMoving => piecesMovementManager.ArePiecesMoving;

        public override void Collapse()
        {
            int iterationAxis = (board.CollapseDirection.x != 0) ? 1 : 0;
            bool colapsed = false;
            for (int lineIndex = 0; lineIndex < board.Dimensions[iterationAxis]; lineIndex++)
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

        private void CallCollapseEvent()
        {
            piecesMovementManager.OnPiecesMovementStopped -= CallCollapseEvent;
            OnPiecesColapsed?.Invoke();
        }

        private int CollapseTokensInLine(int lineIndex, int iterationAxis)
        {
            int collapseAxis = 1 - iterationAxis; // to samo
            int lineSize = board.Dimensions[collapseAxis]; // to samo

            int startCellIndex = board.CollapseDirection[collapseAxis] > 0 ? -1 : 0; // odwrócony warunek
            int lineCollapseDirection = board.CollapseDirection[collapseAxis] == 0 ? 1 : -board.CollapseDirection[collapseAxis];

            int nonExistingTokensCount = 0; // inna rzecz
            for (int i = 0; i < lineSize; i++) // troszkę inne
            {
                var coord = Vector2Int.zero; // to samo
                coord[iterationAxis] = lineIndex; // to samo
                coord[collapseAxis] = (startCellIndex + i * lineCollapseDirection + lineSize) % lineSize; // odwrócony znak

                // odtąd są inne czynnności
                var piece = Board.GetPiece(coord);
                if (piece == null || piece.IsCleared)
                {
                    nonExistingTokensCount++;
                }
                else if (nonExistingTokensCount > 0)
                {
                    var offsetToMove = board.CollapseDirection * nonExistingTokensCount;
                    var targetCoord = coord + offsetToMove;
                    Board[coord] = null;
                    Board[targetCoord] = piece;
                    piecesMovementManager.StartPieceMovement(piece, targetCoord, 0.3f); // to samo
                }
            }

            return nonExistingTokensCount;
        }

        private void RefillLine(int lineIndex, int count, int iterationAxis)
        {
            int collapseAxis = 1 - iterationAxis; // to samo
            int lineSize = board.Dimensions[collapseAxis]; // to samo

            int startCellIndex = board.CollapseDirection[collapseAxis] < 0 ? -1 : 0; // odwrócony warunek
            var spawnOffset = -board.CollapseDirection * count; // inna rzecz
            
            int refillingDirection = board.CollapseDirection[collapseAxis] == 0 ? 1 : board.CollapseDirection[collapseAxis];

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

        private System.Action swapEndedCallback;
        public override void SwapTokens(Vector2Int pieceCoord1, Vector2Int pieceCoord2)
        {
            var piece1 = Board.GetPiece(pieceCoord1);
            var piece2 = Board.GetPiece(pieceCoord2);

            piecesMovementManager.StartPieceMovement(piece2, pieceCoord1);
            piecesMovementManager.StartPieceMovement(piece1, pieceCoord2);

            (Board[pieceCoord1], Board[pieceCoord2]) = (Board[pieceCoord2], Board[pieceCoord1]);

            piecesMovementManager.OnPiecesMovementStopped += BoardController_OnPiecesMovementStopped;
            swapEndedCallback = () =>
            {
                swapEndedCallback = null;
                OnPiecesSwapped?.Invoke(pieceCoord1, pieceCoord2);
            };
        }

        private void BoardController_OnPiecesMovementStopped()
        {
            piecesMovementManager.OnPiecesMovementStopped -= BoardController_OnPiecesMovementStopped;
            swapEndedCallback.Invoke();
        }
    }
}
