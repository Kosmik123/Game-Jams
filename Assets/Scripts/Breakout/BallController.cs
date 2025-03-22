using NaughtyAttributes;
using System.Collections;
using UnityEngine;

namespace Bipolar.Breakout
{
    public interface IBall
    {
        Vector2 Velocity { get; }
    }

    [RequireComponent(typeof(Rigidbody2D))]
    public class BallController : MonoBehaviour, IBall
    {
        [SerializeField, ReadOnly]
        private float moveSpeed;
        public float MoveSpeed
        {
            get => moveSpeed;
            set
            {
                moveSpeed = value;
                SetMovement(currentVelocity);
            }
        }

        [SerializeField, ReadOnly]
        private Vector2 currentVelocity;
        public Vector2 Velocity => currentVelocity;

        private Rigidbody2D rigidBody;

        private void Awake()
        {
            rigidBody = GetComponent<Rigidbody2D>();
        }

        private IEnumerator Start()
        {
            yield return new WaitUntil(() => UnityEngine.Input.GetKeyDown(KeyCode.Space));
            SetMovement(Random.insideUnitCircle);
        }

        private void Update()
        {
            currentVelocity = rigidBody.velocity;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            Vector2 direction = collision.gameObject.TryGetComponent<IBallReflector>(out var reflector)
                ? reflector.GetBounceDirection(this, collision)
                : (Vector2)Vector3.Reflect(currentVelocity, collision.GetContact(0).normal);

            moveSpeed *= 1.05f;
            SetMovement(direction);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, currentVelocity);
        }

        public void SetMovement(Vector2 direction)
        {
            var directionNormalized = direction.normalized;
            rigidBody.velocity = directionNormalized * moveSpeed;   
        }
    }

    public interface IBallReflector
    {
         Vector2 GetBounceDirection(IBall ball, Collision2D collision);
    }
}
