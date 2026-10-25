using UnityEngine;
using UnityEngine.UI;

namespace ZeroHour.UI
{
    // UI text that rises by a few pixels and fades out, then destroys itself.
    public class FloatingText : MonoBehaviour
    {
        private Text text;
        private Outline outline;
        private Color color;
        private Color outlineColor;
        private Vector2 startPosition;
        private float rise;
        private float duration;
        private float age;

        public void Init(Color color, float rise, float duration)
        {
            text = GetComponent<Text>();
            outline = GetComponent<Outline>();
            outlineColor = outline != null ? outline.effectColor : Color.black;
            this.color = color;
            this.rise = rise;
            this.duration = duration;
            startPosition = text.rectTransform.anchoredPosition;
            Apply();
        }

        private void Update()
        {
            age += Time.deltaTime;
            if (age >= duration)
            {
                Destroy(gameObject);
                return;
            }
            Apply();
        }

        private void Apply()
        {
            float t = age / duration;
            float alpha = 1f - t * t;
            text.rectTransform.anchoredPosition = startPosition + Vector2.up * (rise * t);
            text.color = new Color(color.r, color.g, color.b, color.a * alpha);
            if (outline != null)
                outline.effectColor = new Color(outlineColor.r, outlineColor.g, outlineColor.b, outlineColor.a * alpha);
        }
    }
}
