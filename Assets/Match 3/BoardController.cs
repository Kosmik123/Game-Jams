using UnityEngine;

namespace Bipolar.Match3
{
    public abstract class BoardController : MonoBehaviour
    {
        public abstract event System.Action OnPiecesColapsed;
        public abstract event PiecesSwapEventHandler OnTokensSwapped;

        public abstract Board Board { get; }

        [SerializeField]
        private PiecesSpawner tokensSpawner;
        public PiecesSpawner TokensSpawner
        {
            get => tokensSpawner;
            set => tokensSpawner = value;
        }

        [SerializeField]
        private PieceTypeProvider tokenTypeProvider;
        public PieceTypeProvider TokenTypeProvider
        {
            get => tokenTypeProvider;
            set => tokenTypeProvider = value;
        }

        public abstract bool AreTokensMoving { get; }

        protected Piece CreateToken(Vector2Int coord)
        {
            var token = TokensSpawner.SpawnPiece();
            token.Type = TokenTypeProvider.GetPieceType(coord.x, coord.y);
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
