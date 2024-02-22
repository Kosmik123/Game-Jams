using System.Collections.Generic;
using UnityEngine;

namespace Bipolar.PuzzleBoard
{
    public interface IPiecesIndexable
    {
        Piece this[Vector2Int coord] { get; set; }
    }

    [DisallowMultipleComponent, RequireComponent(typeof(Grid))]
    public abstract class Board : MonoBehaviour
    {
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

        protected virtual void Awake()
        { }

        public abstract IReadOnlyCollection<Piece> Pieces { get; }

        public abstract Piece this[Vector2Int coord] { get; internal set; }

        public bool Contains(Vector2Int coord) => Contains(coord.x, coord.y);
        public abstract bool Contains(int x, int y);

        public Vector3 CoordToWorld(float x, float y) => CoordToWorld(new Vector2(x, y));

        public virtual Vector3 CoordToWorld(Vector2 coord)
        {
            var localPosition = Grid.CellToLocalInterpolated(coord);
            return transform.TransformPoint(localPosition);
        }

        public virtual Vector2Int WorldToCoord(Vector3 worldPosition)
        {
            switch (Grid.cellLayout)
            {
                case GridLayout.CellLayout.Hexagon:
                    break;
                case GridLayout.CellLayout.Rectangle:
                    worldPosition += 0.5f * (Grid.cellSize + Grid.cellGap);
                    break;
                default: 
                    worldPosition.y += 0.5f * (Grid.cellSize.y + Grid.cellGap.y);
                    break;
            }

            var coord = Grid.WorldToCell(worldPosition);
            return (Vector2Int)coord;
        }

        public Piece GetPiece(int x, int y) => GetPiece(new Vector2Int(x, y));
        public Piece GetPiece(Vector2Int coord)
        {
            if (Contains(coord) == false)
                return null;

            var piece = this[coord];
            if (piece == null || piece.IsCleared)
                return null;
            return piece;
        }

        public static Vector2Int GetFixedDirection(Vector2Int coord, Vector2Int direction, bool isHexagonal)
        {
            if (isHexagonal && direction.y != 0)
            {
                if (coord.y % 2 == 0)
                {
                    if (direction.x > 0)
                        direction.x = 0;
                }
                else
                {
                    if (direction.x <= 0)
                        direction.x += 1;
                }
            }
            return direction;
        }
    }
}
