using System;

namespace Bipolar.PuzzleBoard.General
{
    public class GeneralBoardPiecesMovementManager : PiecesMovementManager
    {
        public override event Action OnPiecesMovementStopped;
        public override bool ArePiecesMoving => false;
    }
}
