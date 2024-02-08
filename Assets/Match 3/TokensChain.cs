using System.Collections.Generic;
using UnityEngine;

namespace Bipolar.Match3
{
    public class TokensChain
    {
        public TokenType TokenType { get; private set; }
        private readonly HashSet<Vector2Int> tokenCoords = new HashSet<Vector2Int>();
        public IReadOnlyCollection<Vector2Int> TokenCoords => tokenCoords;
        public bool IsMatchFound { get; set; } = false;
        public bool Contains(Vector2Int tokenCoord) => tokenCoords.Contains(tokenCoord);
        public int Size => tokenCoords.Count;

        private readonly HashSet<Vector2Int> horizontalLines = new HashSet<Vector2Int>();
        private readonly HashSet<Vector2Int> verticalLines = new HashSet<Vector2Int>();
        public int HorizontalLinesCount => horizontalLines.Count;
        public int VerticalLinesCount => verticalLines.Count;

        public TokensChain(TokenType type)
        {
            TokenType = type;
        }

        public void Add(Vector2Int tokenCoord)
        {
            tokenCoords.Add(tokenCoord);
        }

        public void AddHorizontal(Vector2Int lineCenter)
        {
            horizontalLines.Add(lineCenter);
        }

        public void AddVertical(Vector2Int lineCenter)
        {
            verticalLines.Add(lineCenter);
        }

        public void Clear()
        {
            tokenCoords.Clear();
        }

        public override string ToString()
        {
            return $"Tokens Chain ({TokenType.name}): {Size}, H: {HorizontalLinesCount}, V: {VerticalLinesCount}";
        }
    }
}
