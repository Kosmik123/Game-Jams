using System.Collections.Generic;
using UnityEngine;

namespace Bipolar.Match3
{
    public class PooledTokensSpawner : TokensSpawner
    {
        [SerializeField]
        private Token tokenPrototype;
        [SerializeField]
        private Transform tokensContainer;

        private Stack<Token> tokensPool = new Stack<Token>();

        public override Token SpawnToken()
        {
            var spawnedToken = tokensPool.Count > 0 ? tokensPool.Pop() : CreateNewToken();
            spawnedToken.IsCleared = false;
            spawnedToken.gameObject.SetActive(true);
            return spawnedToken;
        }

        private Token CreateNewToken()
        {
            var token = Instantiate(tokenPrototype, tokensContainer);
            token.OnCleared += Release;
            return token;
        }

        private async void ReleaseAfterFrame(Token token)
        {
            await System.Threading.Tasks.Task.Delay(10);
            Release(token);
        }

        private void Release(Token token)
        {
            token.gameObject.SetActive(false);
            tokensPool.Push(token);
        }
    }
}
