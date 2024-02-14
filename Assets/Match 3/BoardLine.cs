using System.Collections.Generic;
using UnityEngine;

namespace Bipolar.Match3
{
    [System.Serializable]
    public class BoardLine
    {
        [SerializeField]
        private List<Vector2Int> coords = new List<Vector2Int>();
        public IReadOnlyList<Vector2Int> Coords => coords;

        public bool Contains(Vector2Int coord) => coords.Contains(coord);

    }
}
