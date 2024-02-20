using UnityEngine;

namespace Bipolar.PuzzleBoard
{
    public abstract class PiecesMovementManager : MonoBehaviour
    {
        public abstract event System.Action OnPiecesMovementStopped;
        public abstract bool ArePiecesMoving { get; }

        protected readonly WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();
    }
}
