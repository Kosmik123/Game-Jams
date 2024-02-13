using UnityEngine;

namespace Bipolar.Match3
{
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

        protected Token[,] tokens;

        public abstract bool Contains(Vector2Int coord);
        public abstract Vector2Int WorldToCoord(Vector3 worldPosition);
        public Vector3 CoordToWorld(float x, float y) => CoordToWorld(new Vector2(x, y));
        public abstract Vector3 CoordToWorld(Vector2 coord);
    }
}
