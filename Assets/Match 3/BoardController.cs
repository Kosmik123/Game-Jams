using UnityEngine;

namespace Bipolar.Match3
{
    [DisallowMultipleComponent, RequireComponent(typeof(Board), typeof(BoardCollapsing<>))]
    public abstract class BoardController : MonoBehaviour
    {
        public abstract event PiecesSwapEventHandler OnPiecesSwapped;
        public abstract event System.Action OnPiecesColapsed;

        public abstract Board Board { get; }
        public abstract bool ArePiecesMoving { get; }
        public abstract bool IsCollapsing { get; }

        public abstract void Collapse();
        public abstract void SwapTokens(Vector2Int pieceCoord1, Vector2Int pieceCoord2);
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
    }
}
