using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Bipolar.PuzzleBoard.General
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
    }
}
