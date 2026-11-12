#if ENABLE_INPUT_SYSTEM && BIPOLAR_TOUCHSTICKS_INPUT_SYSTEM
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace Bipolar.TouchSticks
{
    /// <summary>Reads touches from the Input System. Without a touchscreen the mouse emulates one finger.</summary>
    public class InputSystemPointerSource : IPointerSource
    {
        private const int MouseId = -1;

        public void Collect(List<Pointer> results)
        {
            var touchscreen = Touchscreen.current;
            if (touchscreen != null)
            {
                CollectTouches(touchscreen, results);
                return;
            }

            var mouse = Mouse.current;
            if (mouse == null)
                return;

            var button = mouse.leftButton;
            if (button.isPressed || button.wasReleasedThisFrame)
                results.Add(new Pointer(MouseId, mouse.position.ReadValue(), button.wasPressedThisFrame, button.wasReleasedThisFrame));
        }

        private static void CollectTouches(Touchscreen touchscreen, List<Pointer> results)
        {
            var touches = touchscreen.touches;
            for (int i = 0; i < touches.Count; i++)
            {
                var touch = touches[i];
                bool released = touch.press.wasReleasedThisFrame;
                if (touch.press.isPressed == false && released == false)
                    continue;

                results.Add(new Pointer(touch.touchId.ReadValue(), touch.position.ReadValue(), touch.press.wasPressedThisFrame, released));
            }
        }
    }
}
#endif
