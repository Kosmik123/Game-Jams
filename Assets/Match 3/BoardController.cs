using UnityEngine;

namespace Bipolar.Match3
{
    public abstract class BoardController : MonoBehaviour
    {
        public abstract Board Board { get; }

        [SerializeField]
        private TokensSpawner tokensSpawner;
        public TokensSpawner TokensSpawner
        {
            get => tokensSpawner;
            set => tokensSpawner = value;
        }

        [SerializeField]
        private TokenTypeProvider tokenTypeProvider;
        public TokenTypeProvider TokenTypeProvider
        {
            get => tokenTypeProvider;
            set => tokenTypeProvider = value;
        }

        protected Token CreateToken(int x, int y)
        {
            var token = TokensSpawner.SpawnToken();
            token.Type = TokenTypeProvider.GetTokenType(x, y);
            return token;
        }

        public abstract void Collapse();
    }

    public abstract class BoardController<T> : BoardController where T : Board
    {
        [SerializeField]
        protected T board;
        public override Board Board => board;
    }
}
