using UnityEngine;

namespace Bipolar.Match3
{
    [DisallowMultipleComponent, RequireComponent(typeof(Board), typeof(BoardCollapsing<>))]
    public abstract class BoardController : MonoBehaviour
    {
        public abstract event PiecesSwapEventHandler OnPiecesSwapped;
        public abstract event System.Action OnPiecesColapsed;

        public abstract Board Board { get; }

        [SerializeField]
        private PiecesSpawner piecesSpawner;
        public PiecesSpawner PiecesSpawner
        {
            get => piecesSpawner;
            set => piecesSpawner = value;
        }

        [SerializeField]
        private PieceTypeProvider pieceTypeProvider;
        public PieceTypeProvider PieceTypeProvider
        {
            get => pieceTypeProvider;
            set => pieceTypeProvider = value;
        }

        public abstract bool ArePiecesMoving { get; }

        protected Piece CreatePiece(Vector2Int coord)
        {
            var piece = PiecesSpawner.SpawnPiece();
            piece.Type = PieceTypeProvider.GetPieceType(coord.x, coord.y);
            Board[coord] = piece;
            return piece;
        }

        public abstract void Collapse();

        public abstract void SwapTokens(Vector2Int pieceCoord1, Vector2Int pieceCoord2);
    }

    public abstract class BoardController<TBoard, TCollapsing> : BoardController
        where TBoard : Board
        where TCollapsing : BoardCollapsing<TBoard>
    {
        public override event System.Action OnPiecesColapsed
        {
            add => Collapsing.OnPiecesColapsed += value;
            remove
            {
                if (Collapsing)
                    Collapsing.OnPiecesColapsed -= value;
            }
        }

        protected TBoard board;
        public override Board Board
        {
            get
            {
                if (board == null)
                    board = GetComponent<TBoard>(); 
                return board;
            }
        }

        protected TCollapsing collapsing;
        public TCollapsing Collapsing
        {
            get
            {
                if (collapsing == null && this)
                    collapsing = GetComponent<TCollapsing>();
                return collapsing;
            }
        }

        protected virtual void Awake()
        {
            board = GetComponent<TBoard>();
        }

        public override void Collapse() => Collapsing.Collapse();
    }
}
