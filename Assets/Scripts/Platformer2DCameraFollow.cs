using UnityEngine;

public class Platformer2DCameraFollow : MonoBehaviour
{
    [SerializeField]
    private Rigidbody2D followedRigidbody;
    [SerializeField]
    private float forwardDistance;

    [SerializeField, Min(0)]
    private float minDetectedSpeed;
    [SerializeField, Min(0)]
    private float moveDelay;
    [SerializeField, Min(0)]
    private float transitionDuration;
    [SerializeField]
    private AnimationCurve transitionCurve;

    private float delayTimer;

    private int direction;
    private float currentCameraOffset;
    private float targetCameraOffset;
    private float previousCameraOffset;

    private float transitionProgress;
    private bool canTransition;

    private void Start()
    {
        direction = 1;
        currentCameraOffset = targetCameraOffset = transform.position.x - followedRigidbody.transform.position.x;
    }

    private void Update()
    {
        int wantedDirection = GetWantedDirection();
        if (direction == 1 && wantedDirection < 0)
        {
            ChangeDirection(-1);
        }
        else if (direction == -1 && wantedDirection > 0)
        {
            ChangeDirection(1);
        }

        delayTimer += Time.deltaTime;
        targetCameraOffset = direction * forwardDistance;

        if (canTransition == false && delayTimer > moveDelay)
        {
            canTransition = true;
            previousCameraOffset = currentCameraOffset;
        }

        if (canTransition)
        {
            float transitionSpeed = 1f / transitionDuration;
            transitionProgress += transitionSpeed * Time.deltaTime;
            currentCameraOffset = Mathf.Lerp(previousCameraOffset, targetCameraOffset, transitionCurve.Evaluate(transitionProgress));
        }
    }

    private int GetWantedDirection()
    {
        float horizontalVelocity = followedRigidbody.velocity.x;
        if (horizontalVelocity < -minDetectedSpeed)
            return -1;

        if (horizontalVelocity > minDetectedSpeed)
            return 1;

        return 0;
    }

    private void ChangeDirection(int dir)
    {
        direction = dir;
        canTransition = false;
        transitionProgress = 0;
        delayTimer = 0;
    }

    private void LateUpdate()
    {
        transform.position = followedRigidbody.transform.position + Vector3.right * currentCameraOffset;
    }
}
