using Bipolar;
using Bipolar.Input;
using UnityEngine;

public class PhysicalPlayerMovement : MonoBehaviour
{
    Rigidbody body;

    [SerializeField]
    private Serialized<IMoveInputProvider> inputProvider;
    public IMoveInputProvider InputProvider => inputProvider.Value;

    [SerializeField]
    private float moveSpeed = 4;

    [SerializeField]
    private Transform forwardProvider;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
    }
    private void FixedUpdate()
    {
        var direction = InputProvider.GetMotion();
        if (direction.sqrMagnitude > 1)
            direction.Normalize();

        var forwardProvider = this.forwardProvider ? this.forwardProvider : transform;
        var direction3D = forwardProvider.forward * direction.y + forwardProvider.right * direction.x;
        body.velocity = moveSpeed * direction3D;
    }
}
