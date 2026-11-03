using UnityEngine;

/// <summary>
/// Moves a creature with a Rigidbody2D. WASD sets the velocity in world space, independently of where the creature
/// faces. The creature turns towards the cursor at a limited speed.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class CreatureMover : MonoBehaviour
{
    [SerializeField]
    private MovementStats stats = MovementStats.Default;

    private Rigidbody2D body;
    private Vector2 moveInput;

    public MovementStats Stats
    {
        get => stats;
        set => stats = value;
    }

    /// <summary>Camera used to find the point the creature should face.</summary>
    public Camera AimCamera { get; set; }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Dynamic;
        body.gravityScale = 0f;
        body.linearDamping = 0f;
        body.angularDamping = 0f;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private void Update()
    {
        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (moveInput.sqrMagnitude > 1f)
        {
            moveInput.Normalize();
        }
    }

    private void FixedUpdate()
    {
        body.linearVelocity = moveInput * stats.moveSpeed;
        body.angularVelocity = 0f;

        if (AimCamera != null)
        {
            Vector2 aimPoint = AimCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 toAim = aimPoint - body.position;
            if (toAim.sqrMagnitude > 0.0001f)
            {
                // The creature's front is its local Up, so an angle of 0 means facing Up.
                float targetAngle = Mathf.Atan2(toAim.y, toAim.x) * Mathf.Rad2Deg - 90f;
                float angle = Mathf.MoveTowardsAngle(body.rotation, targetAngle, stats.rotationSpeed * Time.fixedDeltaTime);
                body.MoveRotation(angle);
            }
        }
    }
}
