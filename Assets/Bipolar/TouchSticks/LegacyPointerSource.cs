#if ENABLE_LEGACY_INPUT_MANAGER
using System.Collections.Generic;
using UnityEngine;

namespace Bipolar.TouchSticks
{
    /// <summary>Reads touches from the legacy Input Manager. On devices without a touchscreen the mouse emulates one finger.</summary>
    public class LegacyPointerSource : IPointerSource
    {
        private const int MouseId = -1;

        public void Collect(List<Pointer> results)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                var touch = Input.GetTouch(i);
                bool ended = touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled;
                results.Add(new Pointer(touch.fingerId, touch.position, touch.phase == TouchPhase.Began, ended));
            }

            if (Input.touchSupported)
                return;

            if (Input.GetMouseButton(0) || Input.GetMouseButtonUp(0))
                results.Add(new Pointer(MouseId, Input.mousePosition, Input.GetMouseButtonDown(0), Input.GetMouseButtonUp(0)));
        }
    }
}
#endif
