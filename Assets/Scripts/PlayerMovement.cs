using Bipolar;
using Bipolar.Input;
using NaughtyAttributes;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D _rigidbody2D;
    public Rigidbody2D Rigidbody
    {
        get
        {
            if (_rigidbody2D == null)
                _rigidbody2D = GetComponent<Rigidbody2D>();
            return _rigidbody2D;
        }
    }
    
    [Header("Movement")]
    [SerializeField, RequireInterface(typeof(IAxisInputProvider))]
    private Object inputProvider;
    public IAxisInputProvider InputProvider => inputProvider as IAxisInputProvider;
    [SerializeField]
    private CapsuleCollider2D groundPoint;
    [SerializeField]
    private float moveSpeed = 4;
    [SerializeField, Range(0, 1)]
    private float midAirModifier;

    [Header("Jump")]
    [SerializeField, RequireInterface(typeof(IActionInputProvider))]
    private Object jumpInputProvider;
    public IActionInputProvider JumpInputProvider => jumpInputProvider as IActionInputProvider;
    [SerializeField]
    private float jumpForce = 10;
    [SerializeField]
    private LayerMask groundLayers;
    [SerializeField, Min(0)]
    private float coyoteDuration;
    [SerializeField]
    private float minJumpInterval;

    [Header("Gravity")]
    [SerializeField]
    private float upGravity = 1;
    [SerializeField]
    private float downGravity = 2;

    [Header("States")]
    [ShowNonSerializedField, ReadOnly]
    private bool isGrounded;

    private float coyoteTimer;
    private float jumpIntervalTimer;
    private bool jump;

    private bool CanJump => isGrounded && jumpIntervalTimer <= 0;

    private void OnEnable()
    {
        JumpInputProvider.OnPerformed += TryJump;  
    }

    private void TryJump()
    {
        if (CanJump)
            jump = true;
    }

    private void Update()
    {
        UpdateGrounded();

        Vector3 velocity = Rigidbody.velocity;
        float horizontal = InputProvider.GetAxis();

        float modifier = isGrounded ? 1 : (midAirModifier * Time.deltaTime);
        velocity.x = Mathf.Lerp(Rigidbody.velocity.x, horizontal * moveSpeed, modifier);


        if (jump)
        {
            jump = false;
            velocity.y = jumpForce;
            jumpIntervalTimer = minJumpInterval;
        }
        jumpIntervalTimer -= Time.deltaTime;

        Rigidbody.gravityScale = velocity.y < 0 ? downGravity : upGravity;
        Rigidbody.velocity = velocity;
    }

    private void UpdateGrounded()
    {
        bool isOnGround = Physics2D.OverlapCapsule((Vector2)groundPoint.transform.position + groundPoint.offset, groundPoint.size, groundPoint.direction, 0, groundLayers);
        if (isOnGround)
            coyoteTimer = coyoteDuration;
        else
            coyoteTimer -= Time.deltaTime;

        isGrounded = coyoteTimer >= 0;
    }

    private void OnDisable()
    {
        JumpInputProvider.OnPerformed -= TryJump;
    }
}
