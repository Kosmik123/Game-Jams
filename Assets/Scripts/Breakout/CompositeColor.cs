using UnityEngine;
using UnityEngine.Events;

namespace Bipolar.Breakout
{
    public class CompositeColor : MonoBehaviour
    {
        [SerializeField]
        private Color color;

        [Space, SerializeField]
        private UnityEvent<Color> changedColors;

        private void Reset()
        {
            color = Color.white;
        }

        private void OnValidate()
        {
            for (int i = 0; i < changedColors.GetPersistentEventCount(); i++)
                changedColors.SetPersistentListenerState(i, UnityEventCallState.EditorAndRuntime);
            changedColors.Invoke(color);
        }
    }
}
