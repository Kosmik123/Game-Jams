using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

namespace Bipolar.Match3
{
    public class GeneralBoard : Board
    {
        [SerializeField]
        private Tilemap tilemap;

        private readonly List<Vector2Int> includedCoords = new List<Vector2Int>();

        private readonly HashSet<int> startingCoordsIndices = new HashSet<int>();
        private readonly HashSet<int> endingCoordsIndices = new HashSet<int>();
        private int[] targetCoordsIndexes;
        private int[] sourceCoordsIndexes;

        private readonly Dictionary<Vector2Int, Token> tokensByCoords = new Dictionary<Vector2Int, Token>();
        public override Token this[Vector2Int coord] 
        { 
            get => tokensByCoords[coord];
            set => tokensByCoords[coord] = value; 
        }

        private readonly Dictionary<Vector2Int, Vector2Int> directions = new Dictionary<Vector2Int, Vector2Int>();

        public Vector2Int GetCoordFromIndex(int index) => includedCoords[index];
        public int GetTargetIndex(int sourceIndex) => targetCoordsIndexes[sourceIndex];
        public int GetSourceIndex(int targetIndex) => sourceCoordsIndexes[targetIndex];

        private CoordsLine[] lines;
        public IReadOnlyList<CoordsLine> Lines => lines;

        public override IReadOnlyCollection<Token> Tokens => tokensByCoords.Values;

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

            var tempTargetCoordsIndexesDict = new Dictionary<int, int>();

            startingCoordsIndices.Clear();
            endingCoordsIndices.Clear();
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

                var direction = tile.Direction;
                directions.Add(coord, direction);
                int coordIndex = includedCoords.IndexOf(coord);
                if (coordIndex < 0)
                {
                    coordIndex = includedCoords.Count;
                    includedCoords.Add(coord);
                }

                if (direction == Vector2Int.zero)
                {
                    startingCoordsIndices.Add(coordIndex);
                    endingCoordsIndices.Add(coordIndex);
                    continue;
                }

                var targetCoord = coord + direction;
                if (TryGetTile(targetCoord, out var targetTile) == false)
                    continue;

                int targetCoordIndex = includedCoords.IndexOf(targetCoord);
                if (targetCoordIndex < 0)
                {
                    targetCoordIndex = includedCoords.Count;
                    includedCoords.Add(targetCoord);
                }

                startingCoordsIndices.Add(coordIndex);
                notEndingCoords.Add(coordIndex);

                endingCoordsIndices.Add(targetCoordIndex);
                notStartingCoords.Add(targetCoordIndex);

                tempTargetCoordsIndexesDict.Add(coordIndex, targetCoordIndex);
            }

            startingCoordsIndices.ExceptWith(notStartingCoords);
            endingCoordsIndices.ExceptWith(notEndingCoords);

            int indexesCount = tempTargetCoordsIndexesDict.Keys.Max() + 1;
            targetCoordsIndexes = new int[indexesCount];
            sourceCoordsIndexes = new int[indexesCount];
            for (int i = 0; i < indexesCount; i++)
            {
                targetCoordsIndexes[i] = -1;
                sourceCoordsIndexes[i] = -1;
            }
            foreach (var kvp in tempTargetCoordsIndexesDict)
            {
                targetCoordsIndexes[kvp.Key] = kvp.Value;
                sourceCoordsIndexes[kvp.Value] = kvp.Key;
            }

            lines = new CoordsLine[startingCoordsIndices.Count];
            int lineIndex = 0;
            foreach (var index in startingCoordsIndices)
            {
                lines[lineIndex] = CreateCoordsLine(index);
                lineIndex++;
            }
        }

        private bool TryGetTile(Vector2Int coord, out GeneralBoardTile tile)
        {
            tile = tilemap.GetTile<GeneralBoardTile>((Vector3Int)coord);
            return tile != null;
        }

        private CoordsLine CreateCoordsLine(int startingIndex)
        {
            var coordsList = new List<Vector2Int>();

            int index = startingIndex;
            do 
            {
                var coord = includedCoords[index];
                coordsList.Add(coord);
                index = targetCoordsIndexes[index];
                if (index < 0)
                    break;
            }
            while (true);
            return new CoordsLine(coordsList);
        }

        public override bool Contains(int x, int y)
        {
            return tilemap.cellBounds.Contains(new Vector3Int(x, y, tilemap.cellBounds.z))
                && tokensByCoords.ContainsKey(new Vector2Int(x, y));
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

        private void OnDrawGizmosSelected()
        {
            if (targetCoordsIndexes != null && includedCoords.Count > 0)
            {
                Gizmos.color = Color.yellow;
                for (int sourceIndex = 0; sourceIndex < targetCoordsIndexes.Length; sourceIndex++)
                {
                    int targetIndex = targetCoordsIndexes[sourceIndex];
                    if (targetIndex < 0)
                        continue;

                    var start = CoordToWorld(includedCoords[sourceIndex]);
                    var target = CoordToWorld(includedCoords[targetIndex]);
                    Gizmos.DrawLine(start, target);
                }

                Gizmos.color = Color.green;
                foreach (var index in startingCoordsIndices)
                {
                    var coord = includedCoords[index];
                    if (TryGetTile(coord, out var tile))
                        Gizmos.DrawSphere(CoordToWorld(coord) - (Vector3)(0.1f * (Vector2)tile.Direction), 0.1f);
                }

                Gizmos.color = Color.red;
                foreach (var index in endingCoordsIndices)
                {
                    var coord = includedCoords[index];
                    if (TryGetTile(coord, out var tile))
                        Gizmos.DrawSphere(CoordToWorld(coord) + (Vector3)(0.1f * (Vector2)tile.Direction), 0.1f);
                }
            }
        }
/*
        public class CoordsCollection : IEnumerable<Vector2Int>
        {
            private IReadOnlyList<Vector2Int> elements;
            public IReadOnlyCollection<int> indices;

            public CoordsCollection(IReadOnlyList<Vector2Int> elements, IReadOnlyCollection<int> indices)
            {
                this.elements = elements;
                this.indices = indices;
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            public IEnumerator<Vector2Int> GetEnumerator() => new CoordEnumerator(elements, indices);
        }
        public class CoordEnumerator : IEnumerator<Vector2Int>
        {
            private IReadOnlyList<Vector2Int> elements;
            private IEnumerator<int> indexEnumerator; 

            public CoordEnumerator(IReadOnlyList<Vector2Int> elements, IReadOnlyCollection<int> indices)
            {
                this.elements = elements;
                indexEnumerator = indices.GetEnumerator();
            }

            public Vector2Int Current => elements[indexEnumerator.Current];
            object IEnumerator.Current => Current;
            public void Dispose() { }
            public bool MoveNext() => indexEnumerator.MoveNext();
            public void Reset() => indexEnumerator.Reset();
        }*/

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
