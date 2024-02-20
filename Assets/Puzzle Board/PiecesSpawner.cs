using UnityEngine;

namespace Bipolar.PuzzleBoard
{
    public abstract class PiecesSpawner : MonoBehaviour
    {
        public abstract Piece SpawnPiece();
    }
}
