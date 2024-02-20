using UnityEngine;

namespace Bipolar.Match3
{
    [RequireComponent(typeof(GeneralBoard), typeof(LinearGeneralBoardCollapsing))]
    public class GeneralBoardController : BoardController<GeneralBoard>
    {
        public override event PiecesSwapEventHandler OnPiecesSwapped;

        [SerializeField]
        private GeneralBoardPiecesMovementManager piecesMovementManager;
        public override bool ArePiecesMoving => piecesMovementManager.ArePiecesMoving;

        protected override void Awake()
        {
            base.Awake();
            (Collapsing as LinearGeneralBoardCollapsing).Init(piecesMovementManager);
        }

        public override void SwapTokens(Vector2Int pieceCoord1, Vector2Int pieceCoord2)
        {
            var piece1 = Board.GetPiece(pieceCoord1);
            var piece2 = Board.GetPiece(pieceCoord2);
            piece2.transform.position = Board.CoordToWorld(pieceCoord1);
            piece1.transform.position = Board.CoordToWorld(pieceCoord2);
            
            (Board[pieceCoord1], Board[pieceCoord2]) = (Board[pieceCoord2], Board[pieceCoord1]);
            OnPiecesSwapped?.Invoke(pieceCoord1, pieceCoord2);
        }
    }
}
