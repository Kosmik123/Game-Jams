using UnityEngine;

namespace Bipolar.Match3
{
    [RequireComponent(typeof(Token))]
    public abstract class TokenClearingBehavior : MonoBehaviour
    {
        public abstract void ClearToken();
    }
}
