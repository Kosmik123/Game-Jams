using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bipolar.TouchSticks
{
    /// <summary>
    /// Shared per-frame snapshot of all pointers. Input is read at most once per frame, no matter how many sticks exist.
    /// Pointers are also claimed here, so two sticks never grab the same finger.
    /// </summary>
    public static class TouchPointers
    {
        private const string TouchUrlFlag = "touch=1";

        private static readonly List<Pointer> pointers = new List<Pointer>();
        private static readonly HashSet<int> claimedIds = new HashSet<int>();
        private static IPointerSource source;
        private static int lastGatherFrame = -1;
        private static bool? enabled;

        /// <summary>Pointers of the current frame (valid after <see cref="Gather"/>).</summary>
        public static IReadOnlyList<Pointer> Pointers => pointers;

        /// <summary>
        /// True on mobile platforms or when the page address contains "touch=1" (then the mouse acts as one finger).
        /// </summary>
        public static bool Enabled
        {
            get
            {
                if (enabled.HasValue == false)
                    enabled = Application.isMobilePlatform || Application.absoluteURL.IndexOf(TouchUrlFlag, StringComparison.OrdinalIgnoreCase) >= 0;

                return enabled.Value;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            pointers.Clear();
            claimedIds.Clear();
            source = null;
            lastGatherFrame = -1;
            enabled = null;
        }

        /// <summary>Replaces the input backend (for example with an Input System one).</summary>
        public static void SetSource(IPointerSource newSource)
        {
            source = newSource;
            lastGatherFrame = -1;
        }

        /// <summary>Reads pointers from the source unless it already happened in this frame.</summary>
        public static void Gather()
        {
            if (lastGatherFrame == Time.frameCount)
                return;

            lastGatherFrame = Time.frameCount;
            pointers.Clear();
            source ??= CreateDefaultSource();
            source?.Collect(pointers);
        }

        /// <summary>Marks the pointer as used by one stick. Returns false if another stick already holds it.</summary>
        public static bool TryClaim(int pointerId)
        {
            return claimedIds.Add(pointerId);
        }

        public static void Release(int pointerId)
        {
            claimedIds.Remove(pointerId);
        }

        private static IPointerSource CreateDefaultSource()
        {
#if ENABLE_INPUT_SYSTEM && BIPOLAR_TOUCHSTICKS_INPUT_SYSTEM
            return new InputSystemPointerSource();
#elif ENABLE_LEGACY_INPUT_MANAGER
            return new LegacyPointerSource();
#else
            return null;
#endif
        }
    }
}
