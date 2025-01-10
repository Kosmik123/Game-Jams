using UnityEngine;

namespace Bipolar.Breakout
{
    public class PaddleCollision : MonoBehaviour, IBallReflector
    {
        [SerializeField]
        private AnimationCurve bounceAngleCurve = new AnimationCurve(new Keyframe[]
        {
            new Keyframe(-1, -1),
            new Keyframe(1, 1)
        });

        [SerializeField]
        private float angleMultiplier = 45;

        [SerializeField, Min(0.001f)]
        private float width = 2;

        public Vector2 GetBounceDirection(IBall ball, Collision2D collision)
        {
            float offset = collision.otherRigidbody.gameObject.transform.position.x - collision.transform.position.x;
            float normalizedOffset = 2 * offset / width;
            float angle = bounceAngleCurve.Evaluate(normalizedOffset) * angleMultiplier;
            Debug.Log($"Angle: {angle}");
            Vector2 newVelocity = Quaternion.AngleAxis(angle, Vector3.back) * Vector2.up;
            return newVelocity;
        }
    }
}
