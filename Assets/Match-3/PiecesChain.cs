using Bipolar.PuzzleBoard;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Bipolar.Match3
{
    public class PiecesChain
    {
        public PieceType PieceType { get; set; }

        protected readonly HashSet<Vector2Int> piecesCoords = new HashSet<Vector2Int>();
        public IReadOnlyCollection<Vector2Int> PiecesCoords => piecesCoords;
        public bool IsMatchFound { get; set; } = false;
        public bool Contains(Vector2Int pieceCoord) => piecesCoords.Contains(pieceCoord);
        public int Size => piecesCoords.Count;

        public void Add(Vector2Int pieceCoord)
        {
            piecesCoords.Add(pieceCoord);
        }

        public virtual void Clear()
        {
            PieceType = null;
            piecesCoords.Clear();
        }

        public override string ToString() => $"Pieces Chain ({PieceType.name}): {Size}";

        public virtual void DrawGizmo(Board board)
        {
            foreach (var coord in PiecesCoords)
                Gizmos.DrawSphere(board.CoordToWorld(coord), 0.3f);
        }

    }

    public class TriosPiecesChain : PiecesChain
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

        public override string ToString() => $"{base.ToString()}, H: {HorizontalTriosCount}, V: {VerticalTriosCount}";
    }
}
