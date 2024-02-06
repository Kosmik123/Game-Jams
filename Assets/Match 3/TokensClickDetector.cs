using UnityEngine;
using UnityEngine.EventSystems;

namespace Bipolar.Match3
{
    public class TokensClickDetector : MonoBehaviour, IPointerClickHandler
    {
        public event System.Action<Token> OnTokenClicked;

        [SerializeField]
        private Board board;

        public void OnPointerClick(PointerEventData eventData)
        {
            var pressWorldPosition = eventData.pointerPressRaycast.worldPosition;
            var pressedToken = board.GetTokenAtPosition(pressWorldPosition);
            if (pressedToken == null)
                return;

            var releaseWorldPosition = eventData.pointerCurrentRaycast.worldPosition;
            var token = board.GetTokenAtPosition(releaseWorldPosition);
            if (pressedToken != token)
                return;

            OnTokenClicked?.Invoke(token);
        }
    }
}
