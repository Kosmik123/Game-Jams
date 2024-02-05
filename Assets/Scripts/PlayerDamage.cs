using Bipolar.PhysicsEvents;
using NaughtyAttributes;
using UnityEngine;

public class PlayerDamage : MonoBehaviour
{
    public event System.Action OnDamaged;

    [SerializeField]
    private OnTriggerEnter2DEvent triggerEnterEvent;

    [SerializeField, Tag]
    private string damagingTag;

    private void OnEnable()
    {
        triggerEnterEvent.OnEvent += TriggerEnterEvent_OnEvent;
    }

    private void TriggerEnterEvent_OnEvent(Collider2D collision)
    {
        if (collision.CompareTag(damagingTag))
        {
            Debug.LogError("DAMAGE!");
            OnDamaged?.Invoke();
        }
    }

    private void OnDisable()
    {
        triggerEnterEvent.OnEvent -= TriggerEnterEvent_OnEvent;
    }
}
