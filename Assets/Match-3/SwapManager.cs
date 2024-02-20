using Bipolar.PuzzleBoard;
using UnityEngine;

namespace Bipolar.Match3
{
    public class SwapManager : MonoBehaviour
    {
        public event PiecesSwapEventHandler OnSwapRequested;
        public event System.Action<Vector2Int> OnPieceSelected;

        [SerializeField]
        private Board board;
        [SerializeField]
        private PiecesClickDetector piecesClickDetector;
        [SerializeField]
        private SwipeDetector swipeDetector;

        [SerializeField]
        private Vector2Int selectedPieceCoord = -Vector2Int.one;

        private void OnEnable()
        {
            swipeDetector.OnPieceSwiped += SwipeDetector_OnPieceSwiped;
            piecesClickDetector.OnPieceClicked += PiecesClickDetector_OnPieceClicked;
        }

        private void PiecesClickDetector_OnPieceClicked(Vector2Int pieceCoord)
        {
            if (TrySwapSelectedPieces(pieceCoord) == false)
            {
                SelectPiece(pieceCoord);
            }
        }

        private void SwipeDetector_OnPieceSwiped(Vector2Int tokenCoord, Vector2Int direction)
        {
            var otherTokenCoord = tokenCoord + direction;
            if (board.Contains(otherTokenCoord) == false)
                return;

            RequestSwap(tokenCoord, otherTokenCoord);
        }

        private bool TrySwapSelectedPieces(Vector2Int tokenCoord)
        {
            if (board.Contains(selectedPieceCoord) == false)
                return false;

            var xDistance = Mathf.Abs(tokenCoord.x - selectedPieceCoord.x);
            var yDistance = Mathf.Abs(tokenCoord.y - selectedPieceCoord.y);
            if ((xDistance != 1 || yDistance != 0) && (xDistance != 0 || yDistance != 1))
                return false;

            RequestSwap(selectedPieceCoord, tokenCoord);
            return true;
        }

        private void SelectPiece(Vector2Int pieceCoord)
        {
            selectedPieceCoord = pieceCoord;
            if (board.Contains(selectedPieceCoord)) 
                OnPieceSelected?.Invoke(selectedPieceCoord);
        }

        private void RequestSwap(Vector2Int pieceCoord1, Vector2Int pieceCoord2)
        {
            SelectPiece(-Vector2Int.one);
            OnSwapRequested?.Invoke(pieceCoord1, pieceCoord2);
        }    

        private void OnDisable()
        {
            piecesClickDetector.OnPieceClicked -= PiecesClickDetector_OnPieceClicked;
            swipeDetector.OnPieceSwiped -= SwipeDetector_OnPieceSwiped;
        }
    }
}
