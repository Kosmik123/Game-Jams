using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Bipolar.PuzzleBoard.General
{
    public class GeneralBoard : Board
    {
        [SerializeField, Tooltip("Provides board shape")]
        private Tilemap tilemap;
        public Tilemap Tilemap => tilemap;

        private List<Vector2Int> includedCoords;
        public IReadOnlyList<Vector2Int> Coords
        {
            get
            {
                if (includedCoords == null) 
                    CreateBoardShape();
                return includedCoords;
            }
        }

        private readonly Dictionary<Vector2Int, Piece> piecesByCoords = new Dictionary<Vector2Int, Piece>();
        public override Piece this[Vector2Int coord] 
        { 
            get => piecesByCoords[coord];
            set => piecesByCoords[coord] = value; 
        }
        public override IReadOnlyCollection<Piece> Pieces => piecesByCoords.Values;

        private void Reset()
        {
            tilemap = GetComponentInChildren<Tilemap>();
        }

        protected override void Awake()
        {
            if (includedCoords == null)
                CreateBoardShape();
        }

        [ContextMenu("Refresh")]
        public void CreateBoardShape()
        {
            includedCoords = new List<Vector2Int>();
            var coordBounds = tilemap.cellBounds;
            for (int y = coordBounds.yMin; y <= coordBounds.yMax; y++)
            {
                for (int x = coordBounds.xMin; x <= coordBounds.xMax; x++)
                {
                    var coord = new Vector2Int(x, y);
                    var tile = tilemap.GetTile((Vector3Int)coord);
                    if (tile != null)
                        includedCoords.Add(coord);
                }
            }
        }

        public override bool Contains(int x, int y)
        {
            return tilemap.cellBounds.Contains(new Vector3Int(x, y, tilemap.cellBounds.z))
                && piecesByCoords.ContainsKey(new Vector2Int(x, y));
        }

        public override Vector3 CoordToWorld(Vector2 coord)
        {
            Vector3 cellPosition = base.CoordToWorld(coord);
            return cellPosition + Grid.Swizzle(Grid.cellSwizzle, tilemap.tileAnchor);
        }

        public override Vector2Int WorldToCoord(Vector3 worldPosition)
        {
            worldPosition -= tilemap.tileAnchor;
            return base.WorldToCoord(worldPosition);
        }

        private void OnDrawGizmosSelected()
        {
            if (includedCoords == null)
                return;

            Gizmos.color = 0.7f * Color.white;
            foreach (var coord in includedCoords)
                Gizmos.DrawSphere(CoordToWorld(coord), 0.3f);
        }
    }
}
