using UnityEngine;

namespace Bipolar.Match3
{
    [RequireComponent(typeof(GeneralBoard), typeof(GeneralBoardCollapsing))]
    public class GeneralBoardController : BoardController<GeneralBoard>
    {
        public override event PiecesSwapEventHandler OnPiecesSwapped;

        [SerializeField]
        private GeneralBoardPiecesMovementManager tokensMovementManager;
        public override bool ArePiecesMoving => tokensMovementManager.ArePiecesMoving;

        protected override void Awake()
        {
            base.Awake();
            (Collapsing as GeneralBoardCollapsing).Init(tokensMovementManager);
        }

        public override void SwapTokens(Vector2Int pieceCoord1, Vector2Int pieceCoord2)
        {
            (Board[pieceCoord1], Board[pieceCoord2]) = (Board[pieceCoord2], Board[pieceCoord1]);
            
            var piece1 = Board.GetPiece(pieceCoord1);
            var piece2 = Board.GetPiece(pieceCoord2);
            piece2.transform.position = Board.CoordToWorld(pieceCoord1);
            piece1.transform.position = Board.CoordToWorld(pieceCoord2);

            OnPiecesSwapped?.Invoke(pieceCoord1, pieceCoord2);
        }
    }
}
