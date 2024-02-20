using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Bipolar.PuzzleBoard
{
    [CreateAssetMenu(menuName = CreateAssetsPath.Root + "Piece Visual Settings")]
    public class PieceVisualSettings : ScriptableObject
    {
        [System.Serializable]
        public class PieceVisualMapping
        {
            public PieceType type;
            public Color color = Color.white;
            public Sprite sprite;
        }

        [SerializeField, FormerlySerializedAs("tokenVisualMappings")]
        private PieceVisualMapping[] pieceVisualMappings;

        private Dictionary<PieceType, Color> pieceVisualColors;
        private Dictionary<PieceType, Sprite> pieceVisualSprites;

        public Color GetPieceColor(PieceType type)
        {
            if (pieceVisualColors == null)
            {
                pieceVisualColors = new Dictionary<PieceType, Color>();
                foreach (var mapping in pieceVisualMappings)
                    pieceVisualColors[mapping.type] = mapping.color;
            }

            return pieceVisualColors[type];
        }

        public Sprite GetPieceSprite(PieceType type)
        {
            if (pieceVisualSprites == null)
            {
                pieceVisualSprites = new Dictionary<PieceType, Sprite>();
                foreach (var mapping in pieceVisualMappings)
                    pieceVisualSprites[mapping.type] = mapping.sprite;
            }

            return pieceVisualSprites[type];
        }
    }
}
