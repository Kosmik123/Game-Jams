using UnityEngine;

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
            var token = Instantiate(tokenPrototype, tokensContainer);
            token.IsDestroyed = false;
            return token;
        }
    }
}
