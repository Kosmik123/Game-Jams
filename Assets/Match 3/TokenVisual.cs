using UnityEngine;

namespace Bipolar.Match3
{
    public class TokenVisual : MonoBehaviour
    {
        [SerializeField]
        private TokenVisualSettings settings;
        [SerializeField] 
        private Token token;
        [SerializeField]
        private SpriteRenderer spriteRenderer;

        private void OnEnable()
        {
            token.OnTypeChanged += Token_OnInitialized;       
        }

        private void Token_OnInitialized(TokenType type)
        {
            spriteRenderer.color = settings.GetTokenColor(type);  
        }

        private void OnDisable()
        {
            token.OnTypeChanged -= Token_OnInitialized;       
        }
    }
}


