using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Bipolar.PuzzleBoard
{
    public class PooledPiecesSpawner : PiecesSpawner
    {
        [SerializeField, FormerlySerializedAs("tokenPrototype")]
        private Piece piecePrototype;
        [SerializeField, FormerlySerializedAs("tokensContainer")]
        private Transform piecesContainer;

        private Stack<Piece> piecesPool = new Stack<Piece>();

        protected override Piece Spawn()
        {
            var spawnedPiece = piecesPool.Count > 0 ? piecesPool.Pop() : CreateNewPiece();
            spawnedPiece.IsCleared = false;
            spawnedPiece.gameObject.SetActive(true);
            return spawnedPiece;
        }

        private Piece CreateNewPiece()
        {
            var piece = Instantiate(piecePrototype, piecesContainer);
            piece.OnCleared += Release;
            return piece;
        }

        private void Release(Piece piece)
        {
            piece.gameObject.SetActive(false);
            piecesPool.Push(piece);
        }
    }
}
