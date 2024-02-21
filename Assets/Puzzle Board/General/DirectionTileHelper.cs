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


        public static Vector2Int GetTileDirection(Vector2Int coord, DirectionTile tile, bool isHexagonal) => GetTileDirection(coord, tile.Direction, isHexagonal);
        public static Vector2Int GetTileDirection(Vector2Int coord, Vector2Int tileDirection, bool isHexagonal)
        {
            if (isHexagonal && tileDirection.y != 0)
            {
                if (coord.y % 2 == 0)
                {
                    if (tileDirection.x > 0)
                        tileDirection.x = 0;
                }
                else
                {
                    if (tileDirection.x <= 0)
                        tileDirection.x += 1;
                }
            }
            return tileDirection;
        }

    }
}