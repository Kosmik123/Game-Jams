using UnityEngine;
using UnityEngine.EventSystems;

namespace Bipolar.PuzzleBoard
{
    public class PiecesClickDetector : MonoBehaviour, IPointerClickHandler
    {
        public event System.Action<Vector2Int> OnPieceClicked;

        [SerializeField]
        private Board board;

        public void OnPointerClick(PointerEventData eventData)
        {
            var pressWorldPosition = eventData.pointerPressRaycast.worldPosition;
            var pressedPieceCoord = board.WorldToCoord(pressWorldPosition);
            if (board.Contains(pressedPieceCoord) == false)
                return;

            var releaseWorldPosition = eventData.pointerCurrentRaycast.worldPosition;
            var pieceCoord = board.WorldToCoord(releaseWorldPosition);
            if (pressedPieceCoord != pieceCoord)
                return;

            OnPieceClicked?.Invoke(pieceCoord);
        }
    }
}
