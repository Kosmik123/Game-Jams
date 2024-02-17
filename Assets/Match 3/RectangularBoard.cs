using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Bipolar.Match3
{
    public class RectangularBoard : Board
    {
        public event System.Action<Vector2Int> OnDimensionsChanged;

        [SerializeField]
        private Vector2Int dimensions;
        public Vector2Int Dimensions
        {
            get => dimensions;
            set
            {
                dimensions = new Vector2Int(Mathf.Max(1, value.x), Mathf.Max(1, value.y));
                CalculateOtherDimensions();
                OnDimensionsChanged?.Invoke(dimensions);
            }
        }

        private Vector3 localCenter;

        [SerializeField]
        private MoveDirection collapseDirection;
        public Vector2Int CollapseDirection
        {
            get
            {
                return collapseDirection switch
                {
                    MoveDirection.Up => Vector2Int.up,
                    MoveDirection.Left => Vector2Int.left,
                    MoveDirection.Right => Vector2Int.right,
                    MoveDirection.Down => Vector2Int.down,
                    _ => Vector2Int.zero,
                };
            }
        }

        private Token[,] tokens;
        public override Token this[Vector2Int coord]
        {
            get => tokens [coord.x, coord.y];
            set => tokens [coord.x, coord.y] = value;
        }

        private TokensCollection tokensCollection = null;
        public override IReadOnlyCollection<Token> Tokens
        {
            get
            {
                tokensCollection ??= new TokensCollection(this);
                return tokensCollection;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            tokens = new Token[dimensions.x, dimensions.y];
            CalculateOtherDimensions();
        }

        private void CalculateOtherDimensions()
        {
            Vector3Int lastCellCoord = (Vector3Int)Dimensions - Vector3Int.one;
            var topRight = Grid.CellToLocal(lastCellCoord);
            if (Grid.cellLayout == GridLayout.CellLayout.Hexagon && ((Vector3Int)Dimensions - Vector3Int.one).y % 2 == 0)
                topRight.x = Grid.CellToLocalInterpolated(lastCellCoord + Vector3.one / 2).x;

            localCenter = topRight / 2; 
        }

        public override Vector3 CoordToWorld(Vector2 coord)
        {
            return base.CoordToWorld(coord) - localCenter;
        }

        public override Vector2Int WorldToCoord(Vector3 worldPosition)
        {
            return base.WorldToCoord(worldPosition + localCenter);
        }

        public override bool Contains(int xCoord, int yCoord)
        {
            if (xCoord < 0 || yCoord < 0)
                return false;

            if (xCoord >= dimensions.x || yCoord >= dimensions.y)
                return false;

            return true;
        }

        private void OnValidate()
        {
            Dimensions = dimensions;
        }

        private void OnDrawGizmosSelected()
        {
            var darkColor = Color.black;
            var lightColor = Color.white;
            var redColor = Color.red;
            lightColor.a = darkColor.a = redColor.a = 0.3f;

            for (int j = 0; j < dimensions.y; j++)
            {
                for (int i = 0; i < dimensions.x; i++)
                {
                    Vector3 position = CoordToWorld(i, j);
                    Vector3 cubeSize = Grid.cellSize;
                    bool isEven = (i + j) % 2 == 0;
                    switch (Grid.cellLayout)
                    {
                        case GridLayout.CellLayout.Rectangle:
                            Gizmos.color = isEven ? darkColor : lightColor;
                            Gizmos.DrawCube(position, cubeSize);
                            break;
                        default:
                            int remainder = (i - (j % 2)) % 3;
                            Gizmos.color = remainder == 0 ? darkColor : remainder == 1 ? lightColor : redColor;
                            Gizmos.DrawSphere(position, cubeSize.z / 3);
                            break;
                    }
                }
            }
        }

        public class TokensCollection : IReadOnlyCollection<Token>
        {
            private readonly RectangularBoard board;

            public TokensCollection(RectangularBoard board) => this.board = board;

            public int Count => board.Dimensions.x * board.Dimensions.y;

            public IEnumerator<Token> GetEnumerator()
            {
                foreach (var token in board.tokens)
                    yield return token;
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
