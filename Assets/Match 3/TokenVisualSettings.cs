using System.Collections.Generic;
using UnityEngine;

namespace Bipolar.Match3
{
    [CreateAssetMenu(menuName = CreateAssetsPath.Root + "Token Visual Settings")]
    public class TokenVisualSettings : ScriptableObject
    {
        [System.Serializable]
        public class TokenVisualMapping
        {
            public TokenType type;
            public Color color = Color.white;
            public Sprite sprite;
        }

        [SerializeField]
        private TokenVisualMapping[] tokenVisualMappings;

        private Dictionary<TokenType, Color> tokenVisualColors;
        private Dictionary<TokenType, Sprite> tokenVisualSprites;

        public Color GetTokenColor(TokenType type)
        {
            if (tokenVisualColors == null)
            {
                tokenVisualColors = new Dictionary<TokenType, Color>();
                foreach (var mapping in tokenVisualMappings)
                    tokenVisualColors[mapping.type] = mapping.color;
            }

            return tokenVisualColors[type];
        }

        public Sprite GetTokenSprite(TokenType type)
        {
            if (tokenVisualSprites == null)
            {
                tokenVisualSprites = new Dictionary<TokenType, Sprite>();
                foreach (var mapping in tokenVisualMappings)
                    tokenVisualSprites[mapping.type] = mapping.sprite;
            }

            return tokenVisualSprites[type];
        }
    }
}
