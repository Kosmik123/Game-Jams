using UnityEngine;

[RequireComponent(typeof(ThreadPhysics))]
public class ThreadLengthConstraint : MonoBehaviour
{
    private ThreadPhysics thread;

    [SerializeField]
    private Rigidbody originRigidbody;
    [SerializeField]
    private Rigidbody endingRigidbody;

    [SerializeField]
    private float maxLength;
    public float MaxLength
    {
        get => maxLength;
        set => maxLength = value;
    }

    [SerializeField]
    private float baseForce;
    [SerializeField]
    private float forceModifier;
    [SerializeField]
    private float damping;

    private void Awake()
    {
        thread = GetComponent<ThreadPhysics>();
    }
   
    private void OnEnable()
    {
        //thread.Origin.TryGetComponent(out originRigidbody);
        //thread.Ending.TryGetComponent(out endingRigidbody);
    }

    private void FixedUpdate()
    {
        ConstrainLength();
    }

    private void ConstrainLength()
    {
        float overlength = thread.Length - maxLength;
        if (overlength < 0)
            return;

        float forceValue = baseForce + overlength * forceModifier;
        PullToThread(forceValue, originRigidbody, false);
        PullToThread(forceValue, endingRigidbody, true);
    }

    private void PullToThread(float forceValue, Rigidbody body, bool isEnding)
    {
        if (body)
        {
            int tipPointIndex = isEnding ? thread.Points.Count - 1 : 0;
            int neighbourIndex = tipPointIndex + (isEnding ? -1 : 1);

            var tipPoint = thread.Points[tipPointIndex];
            var neighbourPoint = thread.Points[neighbourIndex];
            var threadDirection = neighbourPoint - tipPoint;
            threadDirection.Normalize();

            var velocityAlongThread = Vector3.Project(body.velocity, threadDirection);
            Debug.DrawRay(body.position, velocityAlongThread);

            body.AddForceAtPosition(threadDirection * forceValue, tipPoint);
            body.AddForceAtPosition(-velocityAlongThread * damping, tipPoint);
        }
    }
}
