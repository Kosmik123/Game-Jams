using UnityEngine;

namespace Bipolar.Match3
{
    public abstract class TokensMovementManager : MonoBehaviour
    {
        public abstract event System.Action OnTokensMovementStopped;
        public abstract bool AreTokensMoving { get; }

        protected readonly WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();
    }
}
