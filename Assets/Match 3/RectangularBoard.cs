using NaughtyAttributes;
using System;
using UnityEngine;

namespace Bipolar.Match3
{
    [RequireComponent(typeof(Grid))]
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
        private Vector2 localStartCoord;
        public Vector2 RealDimensions { get; private set; }

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

        private void CalculateOtherDimensions()
        {
            localStartCoord = -new Vector2((dimensions.x - 1) / 2f, (dimensions.y - 1) / 2f);
            Vector2 realDimensions = Grid.cellSize + Grid.cellGap;
            realDimensions.Scale(Dimensions);
            RealDimensions = realDimensions;
        }

        public override Vector3 CoordToWorld(Vector2 vector2)
        {
            var gridPosition = localStartCoord + vector2;
            return CellToGlobalInterpolated(gridPosition);
        }

        public override Vector2Int WorldToCoord(Vector3 worldPosition)
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
            lightColor.a = darkColor.a = 0.3f;

            for (int j = 0; j < dimensions.y; j++)
            {
                for (int i = 0; i < dimensions.x; i++)
                {
                    bool isEven = (i + j) % 2 == 0;
                    Gizmos.color = isEven ? lightColor : darkColor; 
                    Vector3 position = CoordToWorld(i, j);
                    Vector3 cubeSize = Grid.cellSize;
                    Gizmos.DrawCube(position, cubeSize);
                }
            }
        }
    }
}
