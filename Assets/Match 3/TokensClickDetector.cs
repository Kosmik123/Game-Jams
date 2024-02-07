using UnityEngine;
using UnityEngine.EventSystems;

namespace Bipolar.Match3
{
    public class TokensClickDetector : MonoBehaviour, IPointerClickHandler
    {
        public event System.Action<Vector2Int> OnTokenClicked;

        [SerializeField]
        private Board board;

        public void OnPointerClick(PointerEventData eventData)
        {
            var pressWorldPosition = eventData.pointerPressRaycast.worldPosition;
            var pressedTokenCoord = board.WorldToCoord(pressWorldPosition);
            if (board.Contains(pressedTokenCoord) == false)
                return;

            var releaseWorldPosition = eventData.pointerCurrentRaycast.worldPosition;
            var tokenCoord = board.WorldToCoord(releaseWorldPosition);
            if (pressedTokenCoord != tokenCoord)
                return;

            OnTokenClicked?.Invoke(tokenCoord);
        }
    }
}
