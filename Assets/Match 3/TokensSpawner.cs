using UnityEngine;

namespace Bipolar.Match3
{
    public class TokensSpawner : MonoBehaviour
    {
        [SerializeField]
        private Token tokenPrototype;
        [SerializeField]
        private Transform tokensContainer;

        public Token SpawnToken()
        {
            var token = Instantiate(tokenPrototype, tokensContainer);
            token.IsDestroyed = false;
            return token;
        }
    }
}
