using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;
using System;

namespace Bipolar.Match3
{
    public class GeneralBoard : Board
    {
        [SerializeField]
        private Tilemap tilemap;

        private readonly HashSet<Vector2Int> includedCoords = new HashSet<Vector2Int>();
        private readonly Dictionary<Vector2Int, Vector2Int> targetCoords = new Dictionary<Vector2Int, Vector2Int>();
        private readonly HashSet<Vector2Int> startingCoords = new HashSet<Vector2Int>();

        private readonly Dictionary<Vector2Int, Token> tokensByCoords = new Dictionary<Vector2Int, Token>();

        public IReadOnlyCollection<Vector2Int> StartingCoords => startingCoords;

        public override Token this[Vector2Int coord] 
        { 
            get => tokensByCoords[coord];
            set => tokensByCoords[coord] = value; 
        }

        private void Reset()
        {
            tilemap = GetComponentInChildren<Tilemap>();
        }

        private void Awake()
        {
            RefreshBoard();
        }

        [ContextMenu("Refresh")]
        public void RefreshBoard()
        {
            includedCoords.Clear();
            var coordBounds = tilemap.cellBounds;
            var remainingCoordsToDetermine = new HashSet<Vector2Int>();

            startingCoords.Clear();
            targetCoords.Clear();
            for (int y = coordBounds.yMin; y <= coordBounds.yMax; y++)
            {
                for (int x = coordBounds.xMin; x <= coordBounds.xMax; x++)
                {
                    var coord = new Vector2Int(x, y);
                    remainingCoordsToDetermine.Add(coord);
                    startingCoords.Add(coord);
                }
            }

            while (remainingCoordsToDetermine.Count > 0)
            {
                var coord = remainingCoordsToDetermine.First();
                remainingCoordsToDetermine.Remove(coord);

                var tile = tilemap.GetTile<GeneralBoardTile>((Vector3Int)coord);
                if (tile == null)
                {
                    startingCoords.Remove(coord);
                    continue;
                }
                includedCoords.Add(coord);

                var direction = tile.Direction;
                if (direction == Vector2Int.zero)
                    continue;

                var targetCoord = coord + direction;
                startingCoords.Remove(targetCoord);
                
                var targetTile = tilemap.GetTile<GeneralBoardTile>((Vector3Int)targetCoord);
                if (targetTile == null)
                    continue;

                includedCoords.Add(targetCoord);
                targetCoords.Add(coord, targetCoord);
            }
        }

        public override bool Contains(int x, int y)
        {
            return tilemap.cellBounds.Contains(new Vector3Int(x, y, tilemap.cellBounds.z))
                && includedCoords.Contains(new Vector2Int(x, y));
        }

        public override Vector3 CoordToWorld(Vector2 coord)
        {
            Vector3 cellPosition = tilemap.CellToWorld(Vector3Int.RoundToInt(coord));
            return cellPosition + Grid.Swizzle(Grid.cellSwizzle, tilemap.tileAnchor);
        }

        public override Token GetToken(int x, int y)
        {
            return null;
        }

        public override Vector2Int WorldToCoord(Vector3 worldPosition)
        {
            worldPosition -= tilemap.tileAnchor;
            return (Vector2Int)tilemap.WorldToCell(worldPosition);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            foreach (var kvp in targetCoords)
            {
                var start = CoordToWorld(kvp.Key);
                var target = CoordToWorld(kvp.Value);
                Gizmos.DrawLine(start, target);
            }

            foreach (var start in startingCoords)
            {
                Gizmos.DrawSphere(CoordToWorld(start), 0.1f);
            }
        }
    }
}
