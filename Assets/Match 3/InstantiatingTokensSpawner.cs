﻿using UnityEngine;

namespace Bipolar.Match3
{
    public abstract class TokensSpawner : MonoBehaviour
    {
        public abstract Token SpawnToken();
    }

    public class InstantiatingTokensSpawner : TokensSpawner
    {
        [SerializeField]
        private Token tokenPrototype;
        [SerializeField]
        private Transform tokensContainer;

        public override Token SpawnToken()
        {
            var spawnedToken = Instantiate(tokenPrototype, tokensContainer);
            spawnedToken.IsCleared = false;
            spawnedToken.OnCleared += token => Destroy(token.gameObject);
            return spawnedToken;
        }
    }
}
