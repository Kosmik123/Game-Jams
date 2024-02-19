using UnityEngine;

namespace Bipolar.Match3
{
    [RequireComponent(typeof(RectangularBoard), typeof(RectangularBoardCollapsing))]
    public class RectangularBoardController : BoardController<RectangularBoard>
    {
        public override event PiecesSwapEventHandler OnPiecesSwapped;

        [SerializeField]
        private RectangularBoardPiecesMovementManager piecesMovementManager;

        public override bool ArePiecesMoving => piecesMovementManager.ArePiecesMoving;
        protected override void Awake()
        {
            base.Awake();
            (Collapsing as RectangularBoardCollapsing).Init(piecesMovementManager);
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
