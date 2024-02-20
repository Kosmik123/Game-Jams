using UnityEngine;

namespace Bipolar.PuzzleBoard
{
    public class InstantiatingPiecesSpawner : PiecesSpawner
    {
        [SerializeField]
        private Piece piecePrototype;
        [SerializeField]
        private Transform piecesContainter;

        public override Piece SpawnPiece()
        {
            var spawnedPiece = Instantiate(piecePrototype, piecesContainter);
            spawnedPiece.IsCleared = false;
            spawnedPiece.OnCleared += piece => Destroy(piece.gameObject);
            return spawnedPiece;
        }
    }
}
