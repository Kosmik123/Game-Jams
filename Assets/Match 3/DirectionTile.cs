using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Bipolar.Match3
{
    [CreateAssetMenu(menuName = CreateAssetsPath.Root + "General Board Tile")]
    public class DirectionTile : Tile
    {
        [SerializeField]
        private Vector2Int direction;
        public Vector2Int Direction => direction;

        [SerializeField]
        private bool jump;
        public bool Jump => jump;

        private void Awake()
        {
            Validate();
        }

        private void OnValidate()
        {
            Validate();
        }

        private void Validate()
        {
            direction = new Vector2Int(Math.Sign(direction.x), Math.Sign(direction.y));
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
