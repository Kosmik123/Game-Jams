using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

namespace Bipolar.Match3
{
    public class GeneralBoard : Board
    {
        [SerializeField, Tooltip("Provides board shape")]
        private Tilemap tilemap;

        private readonly List<Vector2Int> includedCoords = new List<Vector2Int>();
        public IReadOnlyList<Vector2Int> Coords => includedCoords;

        private readonly Dictionary<Vector2Int, Piece> piecesByCoords = new Dictionary<Vector2Int, Piece>();
        public override Piece this[Vector2Int coord] 
        { 
            get => piecesByCoords[coord];
            set => piecesByCoords[coord] = value; 
        }
        public override IReadOnlyCollection<Piece> Pieces => piecesByCoords.Values;


        // TO PRZENIEŚĆ DO COLLAPSING
        private readonly Dictionary<Vector2Int, Vector2Int> directions = new Dictionary<Vector2Int, Vector2Int>();

        private CoordsLine[] lines;
        public IReadOnlyList<CoordsLine> Lines => lines;
        // Aż DOTĄD


        private void Reset()
        {
            tilemap = GetComponentInChildren<Tilemap>();
        }

        protected override void Awake()
        {
            CreateBoardShape();
        }

        public void CreateBoardShape()
        {
            includedCoords.Clear();
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

        [ContextMenu("Refresh")]
        public void RefreshBoardOLD()
        {
            includedCoords.Clear();
            var coordBounds = tilemap.cellBounds;
            var remainingCoordsToDetermine = new HashSet<Vector2Int>();

            var tempSourceCoordsIndexesDict = new Dictionary<int, int>();

            var startingCoordsIndices = new HashSet<int>();
            var endingCoordsIndices = new HashSet<int>();
            directions.Clear();

            for (int y = coordBounds.yMin; y <= coordBounds.yMax; y++)
            {
                for (int x = coordBounds.xMin; x <= coordBounds.xMax; x++)
                {
                    var coord = new Vector2Int(x, y);
                    remainingCoordsToDetermine.Add(coord);
                }
            }

            var notStartingCoords = new HashSet<int>();
            var notEndingCoords = new HashSet<int>();
            while (remainingCoordsToDetermine.Count > 0)
            {
                var coord = remainingCoordsToDetermine.First();
                remainingCoordsToDetermine.Remove(coord);

                if (TryGetTile(coord, out var tile) == false)
                    continue;

                var direction = GetTileDirection(coord, tile, Grid.cellLayout == GridLayout.CellLayout.Hexagon);
                directions.Add(coord, direction);
                int coordIndex = includedCoords.IndexOf(coord);
                if (coordIndex < 0)
                {
                    coordIndex = includedCoords.Count;
                    includedCoords.Add(coord);
                }

                startingCoordsIndices.Add(coordIndex);
                if (direction == Vector2Int.zero)
                {
                    endingCoordsIndices.Add(coordIndex);
                    continue;
                }

                var targetCoord = coord + direction;
                if (tile.Jump)
                {
                    bool hasJumpTarget = false;
                    while (TilemapContainsCoord(tilemap, targetCoord))
                    {
                        if (TryGetTile(targetCoord, out _))
                        {
                            hasJumpTarget = true;
                            break;
                        }
                        targetCoord += GetTileDirection(targetCoord, tile, Grid.cellLayout == GridLayout.CellLayout.Hexagon);
                    }

                    if (hasJumpTarget == false)
                    {
                        endingCoordsIndices.Add(coordIndex);
                        continue;
                    }
                }
                else if (TryGetTile(targetCoord, out _) == false)
                {
                    endingCoordsIndices.Add(coordIndex);
                    continue;
                }

                int targetCoordIndex = includedCoords.IndexOf(targetCoord);
                if (targetCoordIndex < 0)
                {
                    targetCoordIndex = includedCoords.Count;
                    includedCoords.Add(targetCoord);
                }

                notEndingCoords.Add(coordIndex);
                endingCoordsIndices.Add(targetCoordIndex);
                notStartingCoords.Add(targetCoordIndex);

                if (tempSourceCoordsIndexesDict.ContainsKey(targetCoordIndex) == false)
                    tempSourceCoordsIndexesDict.Add(targetCoordIndex, coordIndex);
            }

            startingCoordsIndices.ExceptWith(notStartingCoords);
            endingCoordsIndices.ExceptWith(notEndingCoords);

            var targetCoordsIndexes = CreateCoordsIndexesArray(tempSourceCoordsIndexesDict.Values);
            var sourceCoordsIndexes = CreateCoordsIndexesArray(tempSourceCoordsIndexesDict.Keys);
            foreach (var kvp in tempSourceCoordsIndexesDict)
            {
                sourceCoordsIndexes[kvp.Key] = kvp.Value;
                targetCoordsIndexes[kvp.Value] = kvp.Key;
            }

            lines = new CoordsLine[startingCoordsIndices.Count];
            int lineIndex = 0;
            foreach (var index in startingCoordsIndices)
            {
                lines[lineIndex] = CreateCoordsLine(index, targetCoordsIndexes);
                lineIndex++;
            }
        }

        public static bool TilemapContainsCoord(Tilemap tilemap, Vector2Int coord)
        {
            var bounds = tilemap.cellBounds;
            return coord.y >= bounds.yMin && coord.y < bounds.yMax && coord.x >= bounds.xMin && coord.x < bounds.xMax;
        }

        public static Vector2Int GetTileDirection(Vector2Int coord, GeneralBoardTile tile, bool isHexagonal)
        {
            var direction = tile.Direction;
            if (isHexagonal && direction.y != 0)
            {
                if (coord.y % 2 == 0)
                {
                    if (direction.x > 0)
                        direction.x = 0;
                }
                else
                {
                    if (direction.x <= 0)
                        direction.x += 1;
                }
            }
            return direction;
        }

        private int[] CreateCoordsIndexesArray(IReadOnlyCollection<int> countCollection)
        {
            int count = 0;
            if (countCollection.Count > 0)
                count = countCollection.Max() + 1;

            var array = new int[count];
            for (int i = 0; i < count; i++)
                array[i] = -1;

            return array;
        }

        private bool TryGetTile(Vector2Int coord, out GeneralBoardTile tile)
        {
            tile = tilemap.GetTile<GeneralBoardTile>((Vector3Int)coord);
            return tile != null;
        }

        private CoordsLine CreateCoordsLine(int startingIndex, IReadOnlyList<int> targetCoordsIndexes)
        {
            var coordsList = new List<Vector2Int>();

            int index = startingIndex;
            while (index >= 0)
            {
                var coord = includedCoords[index];
                coordsList.Add(coord);
                if (index >= targetCoordsIndexes.Count)
                    break;
                
                index = targetCoordsIndexes[index];
            }

            return new CoordsLine(coordsList);
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

        public Vector2Int GetDirection(Vector2Int coord) => directions[coord];

        public Vector2 GetRealDirection(Vector2Int coord)
        {
            var direction = GetDirection(coord);
            var nextCoord = coord + direction;

            var worldPos = CoordToWorld(coord);
            var nextWorldPos = CoordToWorld(nextCoord);

            return nextWorldPos - worldPos;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0, 1, 1, 0.5f);
            foreach (var coord in includedCoords)
            {
                Gizmos.DrawSphere(CoordToWorld(coord), 0.4f);
            }

            if (Lines != null && includedCoords.Count > 0)
            {
                foreach (var line in Lines)
                {
                    for (int i = 0; i < line.Coords.Count; i++)
                    {
                        var coord = line.Coords[i];
                        if (i > 0)
                        {
                            var sourceCoord = line.Coords[i - 1];
                            GizmosDrawLineSegment(sourceCoord, coord);
                        }

                        if (i == 0)
                        {
                            GizmosDrawLineStart(coord);
                        }
                        else if (i == line.Coords.Count - 1)
                        {
                            GizmosDrawLineEnd(coord);
                        }
                    }
                }
            }
        }

        private readonly Color orange = (0.5f * Color.red + 0.5f * Color.yellow);
        private void GizmosDrawLineSegment(Vector2Int start, Vector2Int end)
        {
            var startPos = CoordToWorld(start);
            var target = CoordToWorld(end);
            Gizmos.color = orange;
            Gizmos.DrawLine(startPos, target);
        }

        private void GizmosDrawLineStart(Vector2Int coord) => GizmosDrawLineTip(coord, Color.green, -0.1f);
        private void GizmosDrawLineEnd(Vector2Int coord) => GizmosDrawLineTip(coord, Color.red, 0.1f);
        private void GizmosDrawLineTip(Vector2Int coord, Color color, float offset)
        {
            if (TryGetTile(coord, out var tile))
            {
                Gizmos.color = color;
                Gizmos.DrawSphere(CoordToWorld(coord) + (Vector3)(offset * (Vector2)tile.Direction), 0.1f);
            }
        }

        public class CoordsLine
        {
            private Vector2Int[] coords;
            public IReadOnlyList<Vector2Int> Coords => coords;

            public CoordsLine(IEnumerable<Vector2Int> coords)
            {
                this.coords = coords.ToArray();
            }
        }
    }
}
