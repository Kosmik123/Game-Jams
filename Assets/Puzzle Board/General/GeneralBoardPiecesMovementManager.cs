using System;

namespace Bipolar.PuzzleBoard.General
{
    public class GeneralBoardPiecesMovementManager : PiecesMovementManager
    {
        public override event Action OnAllPiecesMovementStopped;
        public override bool ArePiecesMoving => false;
    }
}
