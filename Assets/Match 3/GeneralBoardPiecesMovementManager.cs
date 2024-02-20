using System;

namespace Bipolar.Match3
{
    public class GeneralBoardPiecesMovementManager : PiecesMovementManager
    {
        public override event Action OnPiecesMovementStopped;
        public override bool ArePiecesMoving => false;
    }
}
