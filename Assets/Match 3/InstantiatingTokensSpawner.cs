using System;
using UnityEngine;

namespace Bipolar.Match3
{
    public class InstantiatingTokensSpawner : PiecesSpawner
    {
        [SerializeField]
        private Piece tokenPrototype;
        [SerializeField]
        private Transform tokensContainer;

        public override Piece SpawnPiece()
        {
            var spawnedToken = Instantiate(tokenPrototype, tokensContainer);
            spawnedToken.IsCleared = false;
            spawnedToken.OnCleared += token => Destroy(token.gameObject);
            return spawnedToken;
        }
    }
}
