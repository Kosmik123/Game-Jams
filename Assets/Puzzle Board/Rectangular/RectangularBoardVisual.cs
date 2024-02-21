using UnityEngine;

namespace Bipolar.PuzzleBoard.Rectangular
{
    public class RectangularBoardVisual : MonoBehaviour
    {
        [SerializeField]
        private RectangularBoard board;
        [SerializeField]
        private SpriteRenderer spriteRenderer;

        private void OnEnable()
        {
            if (board)
            {
                RefreshGraphic(board.Dimensions);
                board.OnDimensionsChanged += RefreshGraphic;
            }
        }

        private void RefreshGraphic(Vector2Int dimensions)
        {
            spriteRenderer.transform.localScale = board.Grid.cellSize + board.Grid.cellGap;
            spriteRenderer.size = dimensions;
        }

        private void OnDisable()
        {
            if (board)
                board.OnDimensionsChanged -= RefreshGraphic;
        }

        private void OnValidate()
        {
            if (board)
                RefreshGraphic(board.Dimensions);
        }

        private void OnDrawGizmos()
        {
            if (board)
                RefreshGraphic(board.Dimensions);
        }
    }
}
