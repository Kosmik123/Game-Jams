using UnityEngine;
using UnityEngine.UI;
using PolyAndCode.UI;

namespace Bipolar.UI
{
    public class RecyclableScrollRect : ScrollRect
    {
        public IRecyclableScrollRectDataSource DataSource;

        public bool SelfInitialize = true;

        [SerializeField]
        public ScrollDirection direction;

        [SerializeField]
        private int segments;
        public int Segments
        {
            get => segments;
            set => segments = Mathf.Max(1, value);
        }

        [SerializeField]
        private RectTransform cellTemplate;
        public RectTransform CellTemplate => cellTemplate;

        public RecyclingSystem recyclingSystem;
        private Vector2 prevAnchoredPos;

        public void OnValueChangedListener(Vector2 direction)
        {
            Vector2 dir = content.anchoredPosition - prevAnchoredPos;
            Vector2 contentShift = recyclingSystem.OnValueChangedListener(dir);
            m_ContentStartPosition += contentShift;
            prevAnchoredPos = content.anchoredPosition;
        }

        public void ReloadData(IRecyclableScrollRectDataSource dataSource = null)
        {
            dataSource ??= DataSource;

            if (recyclingSystem != null)
            {
                StopMovement();
                onValueChanged.RemoveListener(OnValueChangedListener);
                recyclingSystem.DataSource = dataSource;
                StartCoroutine(recyclingSystem.InitCoroutine(() =>
                {
                    onValueChanged.AddListener(OnValueChangedListener);
                }));
                prevAnchoredPos = content.anchoredPosition;
            }
        }
    }
}
