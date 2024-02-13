using UnityEngine;
using UnityEngine.EventSystems;

namespace Bipolar.Match3
{
    public class SwipeDetector : MonoBehaviour, IDragHandler, IEndDragHandler
    {
        public delegate void TokenSwipeEventHandler(Vector2Int tokenCoord, Vector2Int direction);
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
            var tokenCoord = board.WorldToCoord(pressWorldPosition);
            if (board.Contains(tokenCoord) == false)
                return;

            var pointerCurrentRaycast = eventData.pointerCurrentRaycast;
            if (pointerCurrentRaycast.isValid == false)
                return;

            var currentWorldPosition = pointerCurrentRaycast.worldPosition;
            var delta = currentWorldPosition - pressWorldPosition;
            if (delta.sqrMagnitude > dragDetectionDistance * dragDetectionDistance)
            {
                canDrag = false;
                OnTokenSwiped?.Invoke(tokenCoord, GetDirectionFromMove(delta));
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
            var tokenCoord = board.WorldToCoord(pressWorldPosition);
            if (board.Contains(tokenCoord) == false)
                return;

            var pointerCurrentRaycast = eventData.pointerCurrentRaycast;
            if (pointerCurrentRaycast.isValid == false)
                return;

            var releaseWorldPosition = pointerCurrentRaycast.worldPosition;
            var delta = releaseWorldPosition - pressWorldPosition;
            if (delta.sqrMagnitude > releaseDetectionDistance * releaseDetectionDistance)
            {
                OnTokenSwiped?.Invoke(tokenCoord, GetDirectionFromMove(delta));
            }
        }

        private Vector2Int GetDirectionFromMove(Vector2 moveDelta)
        {
            // TODO wziąć pod uwagę różne ustawenia swizzle grida (i być może kształt też)
            if (Mathf.Abs(moveDelta.x) > Mathf.Abs(moveDelta.y))
                return moveDelta.x < 0 ? Vector2Int.left : Vector2Int.right; 

            return moveDelta.y < 0 ? Vector2Int.down : Vector2Int.up;
        }
    }
}
