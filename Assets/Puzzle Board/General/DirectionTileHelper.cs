using UnityEngine;
using UnityEngine.Tilemaps;

namespace Bipolar.PuzzleBoard.General
{
    public static class DirectionTileHelper
    {
        public static bool TryGetTile(Vector2Int coord, Tilemap tilemap, out DirectionTile tile)
        {
            tile = tilemap.GetTile<DirectionTile>((Vector3Int)coord);
            return tile != null;
        }


        public static Vector2Int GetTileDirection(Vector2Int coord, DirectionTile tile, bool isHexagonal) => Board.GetFixedDirection(coord, tile.Direction, isHexagonal);

    }
}