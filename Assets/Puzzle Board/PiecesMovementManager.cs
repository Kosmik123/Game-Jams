using System.Collections.Generic;
using UnityEngine;

namespace Bipolar.PuzzleBoard
{
    public interface IPiecesMovementManager
    {
        event System.Action OnAllPiecesMovementStopped;
        bool ArePiecesMoving { get; }
    }

    public abstract class PiecesMovementManager : MonoBehaviour, IPiecesMovementManager
    {
        public abstract event System.Action OnAllPiecesMovementStopped;
        public abstract bool ArePiecesMoving { get; }

        protected readonly WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();
    }
}
