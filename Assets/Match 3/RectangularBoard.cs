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
                var dir = collapseDirection switch
                {
                    MoveDirection.Up => Vector3.up,
                    MoveDirection.Left => Vector3.left,
                    MoveDirection.Right => Vector3.right,
                    MoveDirection.Down => Vector3.down,
                    _ => Vector3.zero,
                };
                return Vector2Int.RoundToInt(Grid.Swizzle(Grid.cellSwizzle, dir));
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
            {
                var right = Grid.Swizzle(Grid.cellSwizzle, Vector3.right);
                var upForward = Grid.Swizzle(Grid.cellSwizzle, new Vector3(0, 1, 1));
                topRight = Vector3.Scale(upForward, topRight);
                topRight += Vector3.Scale(right, Grid.CellToLocalInterpolated(lastCellCoord + Vector3.one / 2));
            }
            localCenter = topRight / 2; 
        }

        public override Vector3 CoordToWorld(Vector2 coord) => base.CoordToWorld(coord) - localCenter;
        public override Vector2Int WorldToCoord(Vector3 worldPosition) => base.WorldToCoord(worldPosition + localCenter);

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
                    case GridLayout.CellLayout.Hexagon:
                        int remainder = (i - (j % 2)) % 3;
                        Gizmos.color = remainder == 0 ? darkColor : remainder == 1 ? lightColor : redColor;
                        Gizmos.DrawSphere(position, cubeSize.z / 2);
                        break;
                    default:
                        Gizmos.color = isEven ? darkColor : lightColor;
                        var matrix = Gizmos.matrix;
                        var isometricRotation = Quaternion.AngleAxis(45, Vector3.forward);
                        Gizmos.matrix = Matrix4x4.TRS(position, isometricRotation, cubeSize / 2);
                        Gizmos.DrawCube(Vector3.zero, Vector3.one);
                        Gizmos.matrix = matrix;
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
