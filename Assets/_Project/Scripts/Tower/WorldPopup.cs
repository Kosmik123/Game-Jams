using UnityEngine;
using UnityEngine.UI;

namespace ZeroHour.Tower
{
    // HUD text pinned to a point in the world (e.g. above a floor): rises and fades out, following the anchor.
    public class WorldPopup : MonoBehaviour
    {
        private Camera worldCamera;
        private Transform anchor;
        private Vector3 offset;
        private Vector3 lastWorldPosition;
        private Text text;
        private Outline outline;
        private Color color;
        private Color outlineColor;
        private float rise;
        private float duration;
        private float age;

        // offset and rise are in world units (not scaled with the anchor).
        public static void Show(TowerHud hud, Camera worldCamera, Transform anchor, Vector3 offset,
            string label, int fontSize, Color color, float rise, float duration)
        {
            Text text = hud.CreateWorldText("World Popup", fontSize);
            text.text = label;
            var popup = text.gameObject.AddComponent<WorldPopup>();
            popup.worldCamera = worldCamera;
            popup.anchor = anchor;
            popup.offset = offset;
            popup.lastWorldPosition = anchor.position + offset;
            popup.text = text;
            popup.outline = text.GetComponent<Outline>();
            popup.color = color;
            popup.outlineColor = popup.outline.effectColor;
            popup.rise = rise;
            popup.duration = duration;
            popup.UpdateView();
        }

        private void LateUpdate()
        {
            age += Time.deltaTime;
            if (age >= duration)
            {
                Destroy(gameObject);
                return;
            }
            UpdateView();
        }

        private void UpdateView()
        {
            // The anchor may collapse and be destroyed; then the text stays where it last was.
            if (anchor)
                lastWorldPosition = anchor.position + offset;

            float t = age / duration;
            Vector3 worldPosition = lastWorldPosition + Vector3.up * (rise * t);
            text.rectTransform.position = worldCamera.WorldToScreenPoint(worldPosition);

            float alpha = 1f - t * t;
            text.color = new Color(color.r, color.g, color.b, color.a * alpha);
            outline.effectColor = new Color(outlineColor.r, outlineColor.g, outlineColor.b, outlineColor.a * alpha);
        }
    }
}
