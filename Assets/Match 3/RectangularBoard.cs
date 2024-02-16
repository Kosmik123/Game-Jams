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

        [field: SerializeField]
        public Vector3 LocalCenter { get; private set; }

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

        private void Awake()
        {
            tokens = new Token[dimensions.x, dimensions.y];
            CalculateOtherDimensions();
        }

        public Vector2 hexAddTest;
        public Vector2 hexScaleTest;
        private void CalculateOtherDimensions()
        {
            Vector3Int lastCellCoord = (Vector3Int)Dimensions - Vector3Int.one;
            var bottomLeft = Grid.CellToLocal(Vector3Int.zero);
            var topRight = Grid.CellToLocal(lastCellCoord);
            if (Grid.cellLayout == GridLayout.CellLayout.Hexagon && ((Vector3Int)Dimensions - Vector3Int.one).y % 2 == 0)
                topRight.x = Grid.CellToLocalInterpolated(lastCellCoord + Vector3.one / 2).x;

            var localCenter = (bottomLeft + topRight) / 2;
            LocalCenter = Grid.Swizzle(Grid.cellSwizzle, localCenter);
        }

        public override Vector3 CoordToWorld(Vector2 coord)
        {
            var localPosition = Grid.CellToLocalInterpolated(coord);
            return transform.TransformPoint(localPosition) - LocalCenter;
        }

        public override Vector2Int WorldToCoord(Vector3 worldPosition)
        {
            var coord = Grid.WorldToCell(worldPosition + LocalCenter);
            return (Vector2Int)coord;
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
                            Gizmos.color = isEven ? lightColor : darkColor;
                            Gizmos.DrawCube(position, cubeSize);
                            break;
                        default:
                            int remainder = (i + j) % 3;
                            //Gizmos.color = remainder == 0 ? darkColor : remainder == 1 ? lightColor : redColor;
                            Gizmos.DrawSphere(position, cubeSize.z / 3);
                            break;
                    }
                }
            }
        }
    }
}
