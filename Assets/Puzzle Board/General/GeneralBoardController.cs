using UnityEngine;

namespace Bipolar.PuzzleBoard.General
{
    [RequireComponent(typeof(GeneralBoard), typeof(LinearGeneralBoardCollapsing))]
    public class GeneralBoardController : BoardController<GeneralBoard>
    {
        public override bool ArePiecesMoving => piecesMovementManager.ArePiecesMoving;
    }
}
