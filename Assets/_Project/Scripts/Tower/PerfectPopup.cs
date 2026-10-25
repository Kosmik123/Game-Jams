using UnityEngine;
using UnityEngine.UI;

namespace ZeroHour.Tower
{
    // "PERFECT!" above a floor that landed perfectly: rises and fades out, following the floor in the world.
    public class PerfectPopup : MonoBehaviour
    {
        private const int FontSize = 70;
        private const float Duration = 0.8f;
        private const float Rise = 1f;
        private const float StartHeight = 0.8f;

        private Camera worldCamera;
        private Transform anchor;
        private Vector3 lastAnchorPosition;
        private Text text;
        private Outline outline;
        private Color outlineColor;
        private float age;

        // combo > 1 adds the multiplier: "PERFECT! x3".
        public static void Show(TowerHud hud, Camera worldCamera, Transform anchor, int combo = 1)
        {
            Text text = hud.CreateWorldText("Perfect Popup", FontSize);
            text.text = combo > 1 ? $"PERFECT! x{combo}" : "PERFECT!";
            var popup = text.gameObject.AddComponent<PerfectPopup>();
            popup.worldCamera = worldCamera;
            popup.anchor = anchor;
            popup.lastAnchorPosition = anchor.position;
            popup.text = text;
            popup.outline = text.GetComponent<Outline>();
            popup.outlineColor = popup.outline.effectColor;
            popup.UpdateView();
        }

        private void LateUpdate()
        {
            age += Time.deltaTime;
            if (age >= Duration)
            {
                Destroy(gameObject);
                return;
            }
            UpdateView();
        }

        private void UpdateView()
        {
            // The floor may collapse and be destroyed; then the text stays where it last was.
            if (anchor)
                lastAnchorPosition = anchor.position;

            float t = age / Duration;
            Vector3 worldPosition = lastAnchorPosition + Vector3.up * (StartHeight + Rise * t);
            text.rectTransform.position = worldCamera.WorldToScreenPoint(worldPosition);

            float alpha = 1f - t * t;
            text.color = new Color(1f, 1f, 1f, alpha);
            outline.effectColor = new Color(outlineColor.r, outlineColor.g, outlineColor.b, outlineColor.a * alpha);
        }
    }
}
