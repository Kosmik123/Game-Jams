using UnityEngine;

namespace Bipolar.Match3
{
    public class PieceVisual : MonoBehaviour
    {
        [SerializeField]
        private PieceVisualSettings settings;
        [SerializeField] 
        private Piece piece;
        [SerializeField]
        private SpriteRenderer spriteRenderer;

        private void OnEnable()
        {
            piece.OnTypeChanged += Piece_OnInitialized;       
        }

        private void Piece_OnInitialized(PieceType type)
        {
            spriteRenderer.color = settings.GetPieceColor(type);
            var sprite = settings.GetPieceSprite(type);
            if (sprite != null)
                spriteRenderer.sprite = sprite;
        }

        private void OnDisable()
        {
            piece.OnTypeChanged -= Piece_OnInitialized;       
        }
    }
}


