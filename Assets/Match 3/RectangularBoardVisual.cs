using UnityEngine;

namespace Bipolar.Match3
{
    public class RectangularBoardVisual : MonoBehaviour
    {
        [SerializeField]
        private RectangularBoard board;
        [SerializeField]
        private SpriteRenderer spriteRenderer;

        private void OnEnable()
        {
            RefreshGraphic(board.Dimensions);
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
