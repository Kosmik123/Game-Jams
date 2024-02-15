using System.Collections.Generic;
using UnityEngine;

namespace Bipolar.Match3
{
    [RequireComponent(typeof(Grid))]
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

        // TODO public abstract IReadOnlyCollection<Token> Tokens { get; }

        public abstract Token this[Vector2Int coord] { get; set; }

        public bool Contains(Vector2Int coord) => Contains(coord.x, coord.y);
        public abstract bool Contains(int x, int y);

        public abstract Vector2Int WorldToCoord(Vector3 worldPosition);
        public Vector3 CoordToWorld(float x, float y) => CoordToWorld(new Vector2(x, y));
        public abstract Vector3 CoordToWorld(Vector2 coord);

        public Token GetToken(int x, int y) => GetToken(new Vector2Int(x, y));
        public Token GetToken(Vector2Int coord)
        {
            if (Contains(coord) == false)
                return null;

            var token = this[coord];
            if (token == null || token.IsCleared)
                return null;

            return token;
        }
    }
}
