using UnityEngine;

namespace Bipolar.PuzzleBoard.Rectangular
{
    [RequireComponent(typeof(RectangularBoard), typeof(RectangularBoardCollapsing))]
    public class RectangularBoardController : BoardController<RectangularBoard>
    {
        public override bool ArePiecesMoving => piecesMovementManager.ArePiecesMoving;

        protected override void Awake()
        {
            base.Awake();
            (Collapsing as RectangularBoardCollapsing).Init(piecesMovementManager);
        }
    }
}
