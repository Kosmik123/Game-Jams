using System;
using UnityEngine;

namespace Bipolar.PuzzleBoard
{
    public delegate void PieceCoordChangeEventHandler(Piece piece, Vector2Int newCoord);

    [DisallowMultipleComponent, RequireComponent(typeof(Board), typeof(BoardCollapsing<>))]
    public abstract class BoardController : MonoBehaviour
    {
        public abstract event System.Action OnPiecesColapsed;

        [SerializeField]
        protected DefaultPiecesMovementManager piecesMovementManager;
        public PiecesMovementManager PiecesMovementManager => piecesMovementManager;

        public abstract Board Board { get; }
        public abstract bool ArePiecesMoving { get; }
        public abstract bool IsCollapsing { get; }
        public abstract IPiecesIndexable Pieces { get; }


        public abstract void Collapse();
    }

    public abstract class BoardController<TBoard> : BoardController
        where TBoard : Board
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

        private BoardCollapsing<TBoard> collapsing;
        public BoardCollapsing<TBoard> Collapsing
        {
            get
            {
                if (collapsing == null && this)
                    collapsing = GetComponent<BoardCollapsing<TBoard>>();
                return collapsing;
            }
        }

        public sealed override bool IsCollapsing => Collapsing.IsCollapsing;

        protected virtual void Awake()
        {
            board = GetComponent<TBoard>();
        }

        public sealed override void Collapse() => Collapsing.Collapse();


        private BoardControllerPiecesIndexable piecesIndexable;
        public override IPiecesIndexable Pieces
        {
            get
            {
                piecesIndexable ??= new BoardControllerPiecesIndexable(
                    (coord) => Board[coord],
                    (coord, piece) =>
                    {
                        if (piece)
                            piecesMovementManager.StartPieceMovement(piece, coord);
                        Board[coord] = piece;
                    });
                return piecesIndexable;
            }
        }

        public class BoardControllerPiecesIndexable : IPiecesIndexable
        {
            private readonly Func<Vector2Int, Piece> getFunction;
            private readonly Action<Vector2Int, Piece> setFunction;

            public BoardControllerPiecesIndexable(Func<Vector2Int, Piece> getFunction, Action<Vector2Int, Piece> setFunction)
            {
                this.getFunction = getFunction;
                this.setFunction = setFunction;
            }

            public Piece this[Vector2Int coord] 
            { 
                get => getFunction(coord); 
                set => setFunction(coord, value);
            }
        }
    }
}
