using Bipolar;
using Bipolar.Input;
using NaughtyAttributes;
using UnityEngine;

public class PhysicalPlayerMovement : MonoBehaviour
{
    private Rigidbody body;

    [SerializeField]
    private Serialized<IMoveInputProvider> inputProvider;
    public IMoveInputProvider InputProvider => inputProvider.Value;

    [SerializeField]
    private float moveSpeed = 4;

    [SerializeField]
    private Transform forwardProvider;

    [Header("Ground")]
    [SerializeField]
    private Transform groundCheck;
    [SerializeField]
    private LayerMask groundLayers = -5;
    [SerializeField]
    private float groundCheckRadius = 0.3f;
    
    [SerializeField, ReadOnly]
    private bool isGrounded;
    public bool IsGrounded => isGrounded;

    [SerializeField]
    private float sprintMultiplier = 1.5f;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        CalculateGrounded();
        if (isGrounded == false)
            return;

        var direction = InputProvider.GetMotion();
        if (direction.sqrMagnitude < 0.001f)
            return;

        if (direction.sqrMagnitude > 1)
            direction.Normalize();

        var forwardProvider = this.forwardProvider ? this.forwardProvider : transform;
        var direction3D = forwardProvider.forward * direction.y + forwardProvider.right * direction.x;
        float speed = moveSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
            speed *= sprintMultiplier;

        var velocity = direction3D * speed;
        velocity.y = body.velocity.y;
        body.velocity = velocity;
    }

    private void CalculateGrounded()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayers);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 0.5f, 0);
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
