using UnityEngine;

namespace ZeroHour.Tower
{
    // Relative control: the platform moves by as much as the finger moved, wherever the finger is.
    // Per-frame deltas (not "start + offset"), so after hitting an edge the way back works at once.
    public class RelativeDrag
    {
        private readonly float sensitivity;
        private bool hasReference;
        private int referencePointerId;
        private float lastPointerX;

        public RelativeDrag(float sensitivity)
        {
            this.sensitivity = sensitivity;
        }

        // Returns the new platform X. A new press or a different pointer only sets the reference point.
        public float Update(bool isPressed, int pointerId, float pointerWorldX, float platformX, float minX, float maxX)
        {
            if (!isPressed)
            {
                hasReference = false;
                return platformX;
            }

            if (!hasReference || pointerId != referencePointerId)
            {
                hasReference = true;
                referencePointerId = pointerId;
                lastPointerX = pointerWorldX;
                return platformX;
            }

            float delta = pointerWorldX - lastPointerX;
            lastPointerX = pointerWorldX;
            return Mathf.Clamp(platformX + delta * sensitivity, minX, maxX);
        }
    }
}
