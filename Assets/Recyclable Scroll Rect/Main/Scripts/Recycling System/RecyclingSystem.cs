using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PolyAndCode.UI
{
    public enum ScrollDirection
    {
        Horizontal = 0,
        Vertical = 1,
    }

    [RequireComponent(typeof(ScrollRect)), DisallowMultipleComponent]
    public class RecyclingSystem : MonoBehaviour
    {
        public IRecyclableScrollRectDataSource DataSource;

        private ScrollRect scrollRect;

        [SerializeField]
        private ScrollDirection direction;

        [SerializeField, Min(1)]
        protected int segmentsCount;
        public int Segments
        {
            get => segmentsCount;
            set
            {
                segmentsCount = Mathf.Max(value, 1);
            }
        }

        [SerializeField]
        private RectTransform prototypeCell;
        public RectTransform PrototypeCell => prototypeCell;

        [SerializeField]
        private bool initializeOnStart = true;

        [Header("Properties")]
        [SerializeField]
        protected Vector2 cellSize;
        public Vector2 CellSize => cellSize;

        [Header("States")]
        [SerializeField]
        protected List<RectTransform> cellPool;
        protected readonly List<ICell> cachedCells = new List<ICell>();
        [SerializeField]
        protected Bounds recyclableViewBounds;

        [SerializeField]
        protected readonly Vector3[] corners = new Vector3[4];
        [SerializeField]
        protected bool isRecycling;

        [SerializeField]
        protected RectTransform viewport, content;
        protected bool IsGrid => segmentsCount != 1;

        [SerializeField]
        protected float minViewportCoverage = 1.6f;
        [SerializeField]
        protected int minPoolSize = 5;

        [SerializeField, Range(0,1)]
        private float recyclingThreshold = 0.25f;

        [SerializeField]
        protected int lastItemIndex;
        [SerializeField]
        protected int startCellIndex, endCellIndex;
        [SerializeField]
        protected int startCellSegment, endCellSegment;

        public int ScrollAxis => (int)direction;
        public int OtherAxis => 1 - (int)direction;

        private void Awake()
        {
            scrollRect = GetComponent<ScrollRect>();
        }

        private void Start()
        {
            scrollRect.horizontal = direction == ScrollDirection.Horizontal;
            scrollRect.vertical = direction == ScrollDirection.Vertical;
            if (initializeOnStart)
                Initialize((scrollRect as RecyclableScrollRect).DataSource);
        }

        public void Initialize(IRecyclableScrollRectDataSource dataSource)
        { 
            if (dataSource == null)
                dataSource = (scrollRect as RecyclableScrollRect).DataSource;

            (scrollRect as RecyclableScrollRect).recyclingSystem = this;
            Init(scrollRect.viewport, scrollRect.content, dataSource);

            scrollRect.onValueChanged.RemoveListener((scrollRect as RecyclableScrollRect).OnValueChangedListener);
            //Adding listener after pool creation to avoid any unwanted recycling behaviour.(rare scenerio)
            StartCoroutine(InitCoroutine(() =>
            {
                scrollRect.onValueChanged.AddListener((scrollRect as RecyclableScrollRect).OnValueChangedListener);
            }));
        }

        protected Vector2 GetDirection(int directionAxis, bool absolute = false)
        {
            return directionAxis == 1 ? (absolute ? Vector2.up : Vector2.down) : Vector2.right;
        }

        public void Init(RectTransform viewport,
            RectTransform content, IRecyclableScrollRectDataSource dataSource)
        {
            this.viewport = viewport;
            this.content = content;
            DataSource = dataSource;
            recyclableViewBounds = new Bounds();
        }

        public IEnumerator InitCoroutine(System.Action onInitialized)
        {
            SetContentAnchor();
            content.anchoredPosition = Vector3.zero;
            yield return null;
            RecalculateRecyclingBounds();

            CreateCellPool();
            lastItemIndex = cellPool.Count;
            startCellIndex = 0;
            endCellIndex = cellPool.Count - 1;

            int numberOfRows = Mathf.CeilToInt(cellPool.Count / (float)segmentsCount);
            float contentSize = numberOfRows * cellSize[ScrollAxis];

            Vector2 sizeDelta = content.sizeDelta;
            sizeDelta[ScrollAxis] = contentSize;
            content.sizeDelta = sizeDelta;

            SetContentAnchor();

            onInitialized?.Invoke();
        }

        public Vector2 OnValueChangedListener(Vector2 scrolledPixels)
        {
            if (isRecycling || cellPool == null || cellPool.Count == 0)
                return Vector2.zero;

            RecalculateRecyclingBounds();

            float sign = GetDirection(ScrollAxis)[ScrollAxis];
            float pixelScroll = scrolledPixels[ScrollAxis];
            float direction = pixelScroll * sign;

            bool IsCellOutsideRecyclingBounds()
            {
                float checkedBorder = GetCheckedBorder(pixelScroll);
                var checkedCell = GetNextRecycledCell(direction);
                float checkedCellEdge = GetCheckedEdge(checkedCell, pixelScroll);
                return pixelScroll * (checkedCellEdge - checkedBorder) > 0;
            }

            if (direction < 0)
            {
                if (IsCellOutsideRecyclingBounds())
                {
                    return RecycleStartToEnd();
                }
            }
            else if (direction > 0)
            {
                if (IsCellOutsideRecyclingBounds())
                {
                    return RecycleEndToStart();
                }
            }

            return Vector2.zero;
        }

        protected float GetCheckedBorder(float worldDirection)
        {
            return worldDirection > 0 ? recyclableViewBounds.max[ScrollAxis] : recyclableViewBounds.min[ScrollAxis];
        }

        protected float GetCheckedEdge(RectTransform cell, float worldDirection)
        {
            return worldDirection > 0 ? Min(cell, ScrollAxis) : Max(cell, ScrollAxis);
        }

        public static float Max(RectTransform rectTransform, int axis)
        {
            return rectTransform.localToWorldMatrix.MultiplyPoint(rectTransform.rect.max)[axis];
        }

        public static float Min(RectTransform rectTransform, int axis)
        {
            return rectTransform.localToWorldMatrix.MultiplyPoint(rectTransform.rect.min)[axis];
        }

        private RectTransform GetNextRecycledCell(float direction)
        {
            return direction > 0 ? cellPool[endCellIndex] : cellPool[startCellIndex];
        }

        protected void ResetPool()
        {
            if (cellPool != null)
            {
                cellPool.ForEach(item => Destroy(item.gameObject));
                cellPool.Clear();
            }
            else
            {
                cellPool = new List<RectTransform>();
            }
            cachedCells.Clear();
        }

        protected RectTransform SpawnCell()
        {
            var item = Instantiate(prototypeCell);
            item.name = "Cell";
            item.sizeDelta = cellSize;
            item.SetParent(content, false);
            return item;
        }

        protected void SetPrototypeCellAnchor()
        {
            if (IsGrid)
            {
                SetTopLeftAnchor(prototypeCell);
            }
            else
            {
                SetStartAnchor(prototypeCell, ScrollAxis);
            }
        }

        protected  void CreateCellPool()
        {
            ResetPool();

            prototypeCell.gameObject.SetActive(true);
            SetPrototypeCellAnchor();

            startCellSegment = endCellSegment = 0;
            float currentViewportCoverage = 0;
            float scrollPos = 0;

            //set new cell size according to its aspect ratio
            cellSize[OtherAxis] = content.rect.size[OtherAxis] / segmentsCount;
            cellSize[ScrollAxis] = prototypeCell.sizeDelta[ScrollAxis] / prototypeCell.sizeDelta[OtherAxis] * cellSize[OtherAxis];

            //Get the required pool coverage and mininum size for the Cell pool
            float requiredCoverage = minViewportCoverage * viewport.rect.size[ScrollAxis] + cellSize[ScrollAxis];
            int poolSize = Mathf.Min(segmentsCount * 3, DataSource.ItemsCount);

            Vector2 scrollDirection = GetDirection(ScrollAxis);
            Vector2 otherDirection = GetDirection(OtherAxis);

            int cellIndex = 0;
            while ((cellPool.Count < poolSize || currentViewportCoverage < requiredCoverage) && cellIndex < DataSource.ItemsCount)
            {
                RectTransform item = SpawnCell();
                cellPool.Add(item);

                if (IsGrid)
                {
                    float otherPos = endCellSegment * cellSize[OtherAxis];
                    item.anchoredPosition = scrollDirection * scrollPos + otherDirection * otherPos;
                    endCellSegment++;
                    if (endCellSegment >= segmentsCount)
                    {
                        endCellSegment = 0;
                        scrollPos += cellSize[ScrollAxis];
                        currentViewportCoverage += item.rect.size[ScrollAxis];
                    }
                }
                else
                {
                    scrollPos = cellIndex * cellSize[ScrollAxis];
                    item.anchoredPosition = scrollDirection * scrollPos;
                    currentViewportCoverage += item.rect.size[ScrollAxis];
                }

                //Setting data for Cell
                var cell = item.GetComponent<ICell>();
                cachedCells.Add(cell);
                DataSource.SetCell(cell, cellIndex);
                cellIndex++;
            }

            if (IsGrid)
            {
                endCellSegment = (endCellSegment - 1 + segmentsCount) % segmentsCount;
            }

            if (prototypeCell.gameObject.scene.IsValid())
            {
                prototypeCell.gameObject.SetActive(false);
            }
        }

        protected void SetStartAnchor(RectTransform rectTransform, int axis)
        {
            Vector2 pos = axis == 1 ? new Vector2(0.5f, 1) : new Vector2(0, 0.5f);
            SetAnchor(rectTransform, pos);
        }

        protected void RecalculateRecyclingBounds()
        {
            if (viewport == null)
                return;

            viewport.GetWorldCorners(corners);
            Vector3 threshold = 0.5f * recyclingThreshold * (corners[2][ScrollAxis] - corners[0][ScrollAxis]) * GetDirection(ScrollAxis, true);
            recyclableViewBounds.min = new Vector3(corners[0].x, corners[0].y) - threshold;
            recyclableViewBounds.max = new Vector3(corners[2].x, corners[2].y) + threshold;
        }

        private void SetContentAnchor()
        {
            SetStartAnchor(content, ScrollAxis);
        }

        protected void SetAnchor(RectTransform rectTransform, Vector2 anchor)
        {
            Vector2 cachedSize = rectTransform.rect.size;

            rectTransform.anchorMin = anchor;
            rectTransform.anchorMax = anchor;
            rectTransform.pivot = anchor;

            rectTransform.sizeDelta = cachedSize;
        }

        protected void SetTopLeftAnchor(RectTransform rectTransform)
        {
            Vector2 pos = new Vector2(0, 1);
            SetAnchor(rectTransform, pos);
        }

        #region RECYCLING
        protected Vector2 RecycleStartToEnd()
        {
            isRecycling = true;
            var scrollDirection = GetDirection(ScrollAxis);
            float sign = scrollDirection[ScrollAxis];
            var scrollAmount = GetDirection(ScrollAxis, true);
            var otherDirection = GetDirection(OtherAxis);

            int n = 0;
            float scrollPos = IsGrid ? cellPool[endCellIndex].anchoredPosition[ScrollAxis] : 0;

            int additionalSegments = 0;
            while (sign * (GetCheckedEdge(cellPool[startCellIndex], -sign) - GetCheckedBorder(-sign)) < 0 && lastItemIndex < DataSource.ItemsCount)
            {
                if (IsGrid)
                {
                    endCellSegment++;
                    if (endCellSegment >= segmentsCount)
                    {
                        n++;
                        endCellSegment = 0;
                        scrollPos = cellPool[endCellIndex].anchoredPosition[ScrollAxis] + sign * cellSize[ScrollAxis];
                        additionalSegments++;
                    }

                    float otherPos = endCellSegment * cellSize[OtherAxis];
                    cellPool[startCellIndex].anchoredPosition = scrollAmount * scrollPos + otherDirection * otherPos;

                    startCellSegment++;
                    if (startCellSegment >= segmentsCount)
                    {
                        startCellSegment = 0;
                        additionalSegments--;
                    }
                }
                else
                {
                    scrollPos = cellPool[endCellIndex].anchoredPosition[ScrollAxis] + sign * cellSize[ScrollAxis];
                    Vector2 cellPosition = cellPool[startCellIndex].anchoredPosition;
                    cellPosition[ScrollAxis] = scrollPos;
                    cellPool[startCellIndex].anchoredPosition = cellPosition;
                }

                //Cell for row at
                DataSource.SetCell(cachedCells[startCellIndex], lastItemIndex);
                lastItemIndex++;

                //set new indices
                endCellIndex = startCellIndex;
                startCellIndex = (startCellIndex + 1) % cellPool.Count;
                if (IsGrid == false)
                    n++;
            }

            if (IsGrid)
            {
                content.sizeDelta += cellSize[ScrollAxis] * additionalSegments * scrollAmount;
                if (additionalSegments > 0)
                {
                    n -= additionalSegments;
                }
            }

            cellPool.ForEach(cell => cell.anchoredPosition -= cellSize[ScrollAxis] * n * scrollDirection);
            content.anchoredPosition += cellSize[ScrollAxis] * n * scrollDirection;
            isRecycling = false;
            return cellSize[ScrollAxis] * n * scrollDirection;
        }

        protected Vector2 RecycleEndToStart()
        {
            isRecycling = true;
            var scrollDirection = GetDirection(ScrollAxis);
            float sign = scrollDirection[ScrollAxis];
            var scrollAmount = GetDirection(ScrollAxis, true);
            var otherDirection = GetDirection(OtherAxis);

            int n = 0;
            float scrollPos = IsGrid ? cellPool[startCellIndex].anchoredPosition[ScrollAxis] : 0;

            int additionalSegments = 0;
            while (sign * (GetCheckedEdge(cellPool[endCellIndex], sign) - GetCheckedBorder(sign)) > 0 && lastItemIndex > cellPool.Count)
            {
                lastItemIndex--;
                if (IsGrid)
                {
                    startCellSegment--;
                    if (startCellSegment < 0)
                    {
                        n++;
                        startCellSegment = segmentsCount - 1;
                        scrollPos = cellPool[startCellIndex].anchoredPosition[ScrollAxis] - sign * cellSize[ScrollAxis];
                        additionalSegments++;
                    }

                    float otherPos = startCellSegment * cellSize[OtherAxis];
                    cellPool[endCellIndex].anchoredPosition = scrollAmount * scrollPos + otherDirection * otherPos;

                    endCellSegment--;
                    if (endCellSegment < 0)
                    {
                        endCellSegment = segmentsCount - 1;
                        additionalSegments--;
                    }
                }
                else
                {
                    scrollPos = cellPool[startCellIndex].anchoredPosition[ScrollAxis] - sign * cellSize[ScrollAxis];
                    Vector2 cellPosition = cellPool[endCellIndex].anchoredPosition;
                    cellPosition[ScrollAxis] = scrollPos;
                    cellPool[endCellIndex].anchoredPosition = cellPosition;
                    n++;
                }


                //Cell for row at
                DataSource.SetCell(cachedCells[endCellIndex], lastItemIndex - cellPool.Count);

                //set new indices
                startCellIndex = endCellIndex;
                endCellIndex = (endCellIndex - 1 + cellPool.Count) % cellPool.Count;
            }

            if (IsGrid)
            {
                content.sizeDelta += cellSize[ScrollAxis] * additionalSegments * scrollAmount;
                if (additionalSegments > 0)
                {
                    n -= additionalSegments;
                }
            }

            cellPool.ForEach(cell => cell.anchoredPosition += cellSize[ScrollAxis] * n * scrollDirection);
            content.anchoredPosition -= cellSize[ScrollAxis] * n * scrollDirection;
            isRecycling = false;
            return -cellSize[ScrollAxis] * n * scrollDirection;
        }
        #endregion

        protected virtual void OnValidate()
        {
            RecalculateRecyclingBounds();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(recyclableViewBounds.center , recyclableViewBounds.size);
        }

        private void OnDisable()
        {
            scrollRect.StopMovement();
        }

        private void OnDestroy()
        {
            foreach (var cell in cellPool)
            {
                Destroy(cell.gameObject);
            }
        }
    }
}
