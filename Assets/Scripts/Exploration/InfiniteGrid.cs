using System.Collections.Generic;
using MicrojamEvolution.GenomeEditor;
using UnityEngine;

namespace MicrojamEvolution.Exploration
{
    /// <summary>
    /// Placeholder background: a checkerboard that covers whatever the camera sees, so movement is visible.
    /// Keeps only as many cells as the view needs, which makes the grid look endless.
    /// </summary>
    public class InfiniteGrid : MonoBehaviour
    {
        private static readonly Color ColorA = new Color(0.12f, 0.13f, 0.16f);
        private static readonly Color ColorB = new Color(0.14f, 0.15f, 0.18f);

        private readonly List<SpriteRenderer> cells = new List<SpriteRenderer>();

        public void Refresh(Camera cam)
        {
            float halfHeight = cam.orthographicSize;
            float halfWidth = halfHeight * cam.aspect;
            Vector3 center = cam.transform.position;
            int minX = Mathf.FloorToInt(center.x - halfWidth) - 1;
            int maxX = Mathf.CeilToInt(center.x + halfWidth) + 1;
            int minY = Mathf.FloorToInt(center.y - halfHeight) - 1;
            int maxY = Mathf.CeilToInt(center.y + halfHeight) + 1;

            int index = 0;
            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    if (index == cells.Count)
                        cells.Add(CreateCell());

                    SpriteRenderer cell = cells[index++];
                    cell.gameObject.SetActive(true);
                    cell.transform.position = new Vector3(x, y, 0f);
                    cell.color = (x + y) % 2 == 0 ? ColorA : ColorB;
                }
            }

            for (; index < cells.Count; index++)
                cells[index].gameObject.SetActive(false);
        }

        private SpriteRenderer CreateCell()
        {
            var go = new GameObject("Cell");
            go.transform.SetParent(transform, false);
            go.transform.localScale = Vector3.one * 0.96f;
            var cellRenderer = go.AddComponent<SpriteRenderer>();
            cellRenderer.sprite = PixelSprites.Square;
            cellRenderer.sortingOrder = -10;
            return cellRenderer;
        }
    }
}
