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
            token.OnInitialized += Token_OnInitialized;       
        }

        private void Token_OnInitialized()
        {
            spriteRenderer.color = settings.GetTokenColor(token.Type);  
        }

        private void OnDisable()
        {
            token.OnInitialized -= Token_OnInitialized;       
        }
    }
}


