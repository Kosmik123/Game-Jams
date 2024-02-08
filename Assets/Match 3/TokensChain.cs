using System.Collections.Generic;
using UnityEngine;

namespace Bipolar.Match3
{
    public class TokensChain
    {
        public TokenType TokenType { get; set; }
        private readonly HashSet<Vector2Int> tokenCoords = new HashSet<Vector2Int>();
        public IReadOnlyCollection<Vector2Int> TokenCoords => tokenCoords;
        public bool IsMatchFound { get; set; } = false;
        public bool Contains(Vector2Int tokenCoord) => tokenCoords.Contains(tokenCoord);
        public int Size => tokenCoords.Count;

        private readonly HashSet<Vector2Int> horizontalTrios = new HashSet<Vector2Int>();
        private readonly HashSet<Vector2Int> verticalTrios = new HashSet<Vector2Int>();
        public int HorizontalTriosCount => horizontalTrios.Count;
        public int VerticalTriosCount => verticalTrios.Count;

        public void Add(Vector2Int tokenCoord)
        {
            tokenCoords.Add(tokenCoord);
        }

        public void AddHorizontal(Vector2Int lineCenter)
        {
            horizontalTrios.Add(lineCenter);
        }

        public void AddVertical(Vector2Int lineCenter)
        {
            verticalTrios.Add(lineCenter);
        }

        public void Clear()
        {
            TokenType = null;
            tokenCoords.Clear();
            verticalTrios.Clear();
            horizontalTrios.Clear();
        }

        public override string ToString()
        {
            return $"Tokens Chain ({TokenType.name}): {Size}, H: {HorizontalTriosCount}, V: {VerticalTriosCount}";
        }
    }
}
