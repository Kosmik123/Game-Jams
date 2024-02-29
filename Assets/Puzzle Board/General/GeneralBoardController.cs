using UnityEngine;

namespace Bipolar.PuzzleBoard.General
{
    [RequireComponent(typeof(GeneralBoard), typeof(BoardCollapsing<GeneralBoard>))]
    public class GeneralBoardController : BoardController<GeneralBoard>
    {
        public override bool ArePiecesMoving => piecesMovementManager.ArePiecesMoving;
    }
}
