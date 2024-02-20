using UnityEngine;

namespace Bipolar.Match3
{
    [RequireComponent(typeof(GeneralBoard), typeof(LinearGeneralBoardCollapsing))]
    public class GeneralBoardController : BoardController<GeneralBoard>
    {
        public override event PiecesSwapEventHandler OnPiecesSwapped;

        [SerializeField]
        private DefaultPiecesMovementManager piecesMovementManager;

        public override bool ArePiecesMoving => piecesMovementManager.ArePiecesMoving;

        private System.Action swapEndedCallback;
        public override void SwapTokens(Vector2Int pieceCoord1, Vector2Int pieceCoord2)
        {
            var piece1 = Board.GetPiece(pieceCoord1);
            var piece2 = Board.GetPiece(pieceCoord2);

            piecesMovementManager.StartPieceMovement(piece1, pieceCoord2);
            piecesMovementManager.StartPieceMovement(piece2, pieceCoord1);

            (Board[pieceCoord1], Board[pieceCoord2]) = (Board[pieceCoord2], Board[pieceCoord1]);
            piecesMovementManager.OnPiecesMovementStopped += PiecesMovementManager_OnPiecesMovementStopped;
            swapEndedCallback = () =>
            {
                swapEndedCallback = null;
                OnPiecesSwapped?.Invoke(pieceCoord1, pieceCoord2);
            };
        }

        private void PiecesMovementManager_OnPiecesMovementStopped()
        {
            piecesMovementManager.OnPiecesMovementStopped -= PiecesMovementManager_OnPiecesMovementStopped;
            swapEndedCallback.Invoke();
        }
    }
}
