using NaughtyAttributes;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private PlayerDamage playerDamage;
    [SerializeField]
    private PlayerMovement playerMovement;

    [SerializeField]
    private float layerChangeDelay;

    [SerializeField, Layer]
    private int deadLayer;
    private int defaultLayer;

    private void Awake()
    {
        defaultLayer = gameObject.layer;
    }

    private void OnEnable()
    {
        playerDamage.OnDamaged += PlayerDamage_OnDamaged;
    }

    private void PlayerDamage_OnDamaged()
    {
        Invoke(nameof(ChangeLayer), layerChangeDelay);
        playerMovement.enabled = false;
        playerMovement.Rigidbody.constraints = RigidbodyConstraints2D.None;
    }

    private void ChangeLayer()
    {
        gameObject.layer = deadLayer;
        if (Mathf.Abs(playerMovement.Rigidbody.angularVelocity) < 0.5f)
            playerMovement.Rigidbody.angularVelocity = 0.5f;
    }

    private void OnDisable()
    {
        playerDamage.OnDamaged -= PlayerDamage_OnDamaged;
    }
}
