using UnityEngine;

namespace Bipolar.PuzzleBoard
{
    public abstract class PiecesSpawner : MonoBehaviour
    {
        public event System.Action<Piece> OnPieceSpawned;

        public Piece SpawnPiece()
        {
            var piece = Spawn();
            OnPieceSpawned?.Invoke(piece);
            return piece;
        }

        protected abstract Piece Spawn();
    }
}
