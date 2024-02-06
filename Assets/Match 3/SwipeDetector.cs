using UnityEngine;
using UnityEngine.EventSystems;

namespace Bipolar.Match3
{
    public class SwipeDetector : MonoBehaviour, IDragHandler, IEndDragHandler
    {
        public delegate void TokenSwipeEventHandler(Token token, MoveDirection direction);
        public event TokenSwipeEventHandler OnTokenSwiped;

        [SerializeField]
        private Board board;

        [SerializeField]
        private float releaseDetectionDistance = 1;
        [SerializeField]
        private float dragDetectionDistance = 2; 

        private bool canDrag = true;

        public void OnDrag(PointerEventData eventData)
        {
            if (canDrag == false)
                return;

            var pressWorldPosition = eventData.pointerPressRaycast.worldPosition;
            var token = board.GetTokenAtPosition(pressWorldPosition);
            if (token == null)
                return;

            var currentWorldPosition = eventData.pointerCurrentRaycast.worldPosition;
            var delta = currentWorldPosition - pressWorldPosition;
            if (delta.sqrMagnitude > dragDetectionDistance * dragDetectionDistance)
            {
                canDrag = false;
                OnTokenSwiped?.Invoke(token, GetDirectionFromMove(delta));
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (canDrag == false)
            {
                canDrag = true;
                return;
            }
            
            var pressWorldPosition = eventData.pointerPressRaycast.worldPosition;
            var token = board.GetTokenAtPosition(pressWorldPosition);
            if (token == null)
                return;

            var releaseWorldPosition = eventData.pointerCurrentRaycast.worldPosition;
            var delta = releaseWorldPosition - pressWorldPosition;
            if (delta.sqrMagnitude > releaseDetectionDistance * releaseDetectionDistance)
            {
                OnTokenSwiped?.Invoke(token, GetDirectionFromMove(delta));
            }
        }

        private MoveDirection GetDirectionFromMove(Vector2 moveDelta)
        {
            // TODO wziąć pod uwagę różne ustawenia swizzle grida (i być może kształt też)
            if (Mathf.Abs(moveDelta.x) > Mathf.Abs(moveDelta.y))
                return moveDelta.x < 0 ? MoveDirection.Left : MoveDirection.Right; 

            return moveDelta.y < 0 ? MoveDirection.Down : MoveDirection.Up;
        }
    }
}
