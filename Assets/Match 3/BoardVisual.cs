using UnityEngine;

namespace Bipolar.Match3
{
    public class BoardVisual : MonoBehaviour
    {
        [SerializeField]
        private Board board;
        [SerializeField]
        private SpriteRenderer spriteRenderer;

        private void OnEnable()
        {
            board.OnDimensionsChanged += RefreshGraphic;
        }

        private void RefreshGraphic(Vector2Int dimensions)
        {
            spriteRenderer.transform.localScale = board.Grid.cellSize + board.Grid.cellGap;
            spriteRenderer.size = dimensions;
        }

        private void OnDisable()
        {
            board.OnDimensionsChanged -= RefreshGraphic;
        }

        private void OnValidate()
        {
            RefreshGraphic(board.Dimensions);
        }

        private void OnDrawGizmos()
        {
            RefreshGraphic(board.Dimensions);
        }
    }
}
