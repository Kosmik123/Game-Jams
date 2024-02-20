using UnityEngine;

namespace Bipolar.PuzzleBoard
{
    [DisallowMultipleComponent, RequireComponent(typeof(Board))]
    public abstract class BoardCollapsing<TBoard>: MonoBehaviour
        where TBoard : Board
    {
        public abstract event System.Action OnPiecesColapsed;

        private TBoard _board;
        public TBoard Board
        {
            get
            {
                if (_board == null)
                    _board = GetComponent<TBoard>();
                return _board;
            }
        }

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

        [SerializeField]
        private bool collapseOnStart;
        public bool CollapseOnStart
        {
            get => collapseOnStart;
            set => collapseOnStart = value;
        }

        protected virtual void Start()
        {
            if (collapseOnStart)
                Collapse();
        }

        public abstract bool IsCollapsing { get; }

        public abstract void Collapse();

        protected Piece CreatePiece(Vector2Int coord)
        {
            var piece = PiecesSpawner.SpawnPiece();
            piece.Type = PieceTypeProvider.GetPieceType(coord.x, coord.y);
            Board[coord] = piece;
            return piece;
        }
    }
}
