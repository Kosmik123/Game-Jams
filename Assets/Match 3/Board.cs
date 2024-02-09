using NaughtyAttributes;
using UnityEngine;

namespace Bipolar.Match3
{
    [RequireComponent(typeof(Grid))]
    public class Board : MonoBehaviour
    {
        public event System.Action<Vector2Int> OnDimensionsChanged;

        private Grid _grid;
        public Grid Grid
        {
            get
            {
                if (_grid == null)
                    _grid = GetComponent<Grid>();
                return _grid;
            }
        }

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
        private Vector2 localStartCoord;
        public Vector2 RealDimensions { get; private set; }

        private Token[,] tokens;

        public Token this[Vector2Int coord]
        {
            get => tokens[coord.x, coord.y];
            set => tokens[coord.x, coord.y] = value;
        }

        private void Awake()
        {
            tokens = new Token[dimensions.x, dimensions.y];
            CalculateOtherDimensions();
        }

        private void CalculateOtherDimensions()
        {
            localStartCoord = -new Vector2((dimensions.x - 1) / 2f, (dimensions.y - 1) / 2f);
            Vector2 realDimensions = Grid.cellSize + Grid.cellGap;
            realDimensions.Scale(Dimensions);
            RealDimensions = realDimensions;
        }

        public Vector3 CoordToWorld(int x, int y) => CoordToWorld(new Vector2(x, y));

        public Vector3 CoordToWorld(Vector2 vector2)
        {
            var gridPosition = localStartCoord + vector2;
            return CellToGlobalInterpolated(gridPosition);
        }

        public Vector2Int WorldToCoord(Vector3 worldPosition)
        {
            var localPosition = transform.InverseTransformPoint(worldPosition);
            var gridPosition = Grid.LocalToCellInterpolated(localPosition);
            var coord = (Vector2)gridPosition - localStartCoord;
            return Vector2Int.RoundToInt(coord);
        }

        private Vector3 CellToGlobalInterpolated(Vector2 cellPosition)
        {
            var localPosition = Grid.CellToLocalInterpolated(cellPosition);
            return transform.TransformPoint(localPosition);
        }

        public void SetTokens(Token[,] tokens)
        {
            for (int j = 0; j < Dimensions.y; j++)
                for (int i = 0; i < Dimensions.x; i++)
                    this.tokens[i, j] = tokens[i, j];
        }

        public void SetToken(Token token, Vector2Int coord)
        {
            tokens[coord.x, coord.y] = token;
        }

        public void SwapTokens(Vector2Int tokenCoord1, Vector2Int tokenCoord2)
        {
            var swapped = (tokens[tokenCoord2.x, tokenCoord2.y], tokens[tokenCoord1.x, tokenCoord1.y]);
            (tokens[tokenCoord1.x, tokenCoord1.y], tokens[tokenCoord2.x, tokenCoord2.y]) = swapped;
        }

        public Token GetTokenAtPosition(Vector3 worldPosition)
        {
            var coord = WorldToCoord(worldPosition);
            return GetToken(coord);
        }

        public bool Contains(Vector2Int coord) => Contains(coord.x, coord.y);

        public bool Contains(int xCoord, int yCoord)
        {
            if (xCoord < 0 || yCoord < 0)
                return false;

            if (xCoord >= dimensions.x || yCoord >= dimensions.y)
                return false;

            return true;
        }

        public Token GetToken(Vector2Int coord) => GetToken(coord.x, coord.y);

        public Token GetToken(int xCoord, int yCoord)
        {
            if (Contains(xCoord, yCoord) == false)
                return null;

            return tokens[xCoord, yCoord];
        }

        private void OnValidate()
        {
            Dimensions = dimensions;
        }

        private void OnDrawGizmosSelected()
        {
            var darkColor = Color.black;
            var lightColor = Color.white;
            lightColor.a = darkColor.a = 0.3f;

            for (int j = 0; j < dimensions.y; j++)
            {
                for (int i = 0; i < dimensions.x; i++)
                {
                    bool isEven = (i + j) % 2 == 0;
                    Gizmos.color = isEven ? lightColor : darkColor; 
                    Vector3 position = CoordToWorld(i, j);
                    Gizmos.DrawCube(position, Grid.cellSize);
                }
            }
        }

    }
}
