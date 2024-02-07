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
        public int HorizontalLinesCount { get; set; }
        public int VerticalLinesCount { get; set; }

        public TokensChain(TokenType type)
        {
            TokenType = type;
        }

        public void Add(Vector2Int tokenCoord)
        {
            tokenCoords.Add(tokenCoord);
        }

        public void Add(TokensChain chain)
        {
            tokenCoords.UnionWith(chain.TokenCoords);
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
