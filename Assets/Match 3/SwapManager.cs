using UnityEngine;

namespace Bipolar.Match3
{
    public class SwapManager : MonoBehaviour
    {
        public event TokensSwapEventHandler OnSwapRequested;

        [SerializeField]
        private Board board;
        [SerializeField]
        private TokensClickDetector tokensClickDetector;
        [SerializeField]
        private SwipeDetector swipeDetector;

        [SerializeField]
        private Vector2Int selectedTokenCoord = -Vector2Int.one;

        private void OnEnable()
        {
            swipeDetector.OnTokenSwiped += SwipeDetector_OnTokenSwiped;
            tokensClickDetector.OnTokenClicked += TokensClickDetector_OnTokenClicked;
        }

        private void TokensClickDetector_OnTokenClicked(Vector2Int tokenCoord)
        {
            if (TrySwapSelectedTokens(tokenCoord) == false)
            {
                selectedTokenCoord = tokenCoord;
            }
        }

        private void SwipeDetector_OnTokenSwiped(Vector2Int tokenCoord, Vector2Int direction)
        {
            var otherTokenCoord = tokenCoord + direction;
            if (board.Contains(otherTokenCoord) == false)
                return;

            RequestSwap(tokenCoord, otherTokenCoord);
        }

        private bool TrySwapSelectedTokens(Vector2Int tokenCoord)
        {
            if (board.Contains(selectedTokenCoord) == false)
                return false;

            var xDistance = Mathf.Abs(tokenCoord.x - selectedTokenCoord.x);
            var yDistance = Mathf.Abs(tokenCoord.y - selectedTokenCoord.y);
            if ((xDistance != 1 || yDistance != 0) && (xDistance != 0 || yDistance != 1))
                return false;

            RequestSwap(selectedTokenCoord, tokenCoord);
            return true;
        }

        private void RequestSwap(Vector2Int tokenCoord1, Vector2Int tokenCoord2)
        {
            selectedTokenCoord = -Vector2Int.one;
            OnSwapRequested?.Invoke(tokenCoord1, tokenCoord2);
        }    

        private void OnDisable()
        {
            tokensClickDetector.OnTokenClicked -= TokensClickDetector_OnTokenClicked;
            swipeDetector.OnTokenSwiped -= SwipeDetector_OnTokenSwiped;
        }
    }
}
