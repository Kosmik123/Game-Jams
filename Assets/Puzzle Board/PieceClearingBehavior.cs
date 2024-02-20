using UnityEngine;

namespace Bipolar.PuzzleBoard
{
    [RequireComponent(typeof(Piece))]
    public abstract class PieceClearingBehavior : MonoBehaviour
    {
        private Piece _piece;
        public Piece Piece
        {
            get
            {
                if (_piece == null)
                    _piece = GetComponent<Piece>();
                return _piece;
            }
        }

        public abstract void ClearPiece();

        protected void FinishClearing()
        {
            Piece.IsCleared = true;
        }
    }
}
