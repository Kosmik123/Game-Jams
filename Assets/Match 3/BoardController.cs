using UnityEngine;

namespace Bipolar.Match3
{
    public abstract class BoardController : MonoBehaviour
    {
        public abstract event System.Action OnTokensColapsed;
        public abstract event TokensSwapEventHandler OnTokensSwapped;

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

        public abstract bool AreTokensMoving { get; }

        protected Token CreateToken(Vector2Int coord)
        {
            var token = TokensSpawner.SpawnToken();
            token.Type = TokenTypeProvider.GetTokenType(coord.x, coord.y);
            Board[coord] = token;
            return token;
        }

        public abstract void Collapse();

        public abstract void SwapTokens(Vector2Int tokenCoord1, Vector2Int tokenCoord2);
    }

    public abstract class BoardController<T> : BoardController where T : Board
    {
        [SerializeField]
        protected T board;
        public override Board Board => board;
    }
}
