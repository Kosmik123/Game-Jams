using System.Collections.Generic;
using UnityEngine;

namespace Bipolar.Match3
{
    public class PiecesChain
    {
        public PieceType TokenType { get; set; }

        protected readonly HashSet<Vector2Int> tokenCoords = new HashSet<Vector2Int>();
        public IReadOnlyCollection<Vector2Int> TokenCoords => tokenCoords;
        public bool IsMatchFound { get; set; } = false;
        public bool Contains(Vector2Int tokenCoord) => tokenCoords.Contains(tokenCoord);
        public int Size => tokenCoords.Count;

        public void Add(Vector2Int tokenCoord)
        {
            tokenCoords.Add(tokenCoord);
        }

        public virtual void Clear()
        {
            TokenType = null;
            tokenCoords.Clear();
        }

        public override string ToString()
        {
            return $"Tokens Chain ({TokenType.name}): {Size}";
        }
    }

    public class TriosTokensChain : PiecesChain
    {
        private readonly HashSet<Vector2Int> horizontalTrios = new HashSet<Vector2Int>();
        private readonly HashSet<Vector2Int> verticalTrios = new HashSet<Vector2Int>();
        public int HorizontalTriosCount => horizontalTrios.Count;
        public int VerticalTriosCount => verticalTrios.Count;
        
        public void AddHorizontal(Vector2Int lineCenter)
        {
            horizontalTrios.Add(lineCenter);
        }

        public void AddVertical(Vector2Int lineCenter)
        {
            verticalTrios.Add(lineCenter);
        }

        public override void Clear()
        {
            base.Clear();
            verticalTrios.Clear();
            horizontalTrios.Clear();
        }

        public override string ToString()
        {
            return base.ToString() + $", H: {HorizontalTriosCount}, V: {VerticalTriosCount}";
        }
    }
}
