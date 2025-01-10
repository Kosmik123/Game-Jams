using UnityEngine;
using Bipolar.Input;
using Mono.Cecil.Cil;

namespace Bipolar.Breakout
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PaddleMovement : MonoBehaviour
    {
        private Rigidbody2D _rigidbody2D;
        public Rigidbody2D Rigidbody => this.GetRequired(ref _rigidbody2D);

        [SerializeField]
        private AxisInputProvider inputProvider;
        [SerializeField]
        private float moveSpeed = 10;
        [SerializeField, Min(0)]
        private float xExtents = 10;

        private float moveDirection;

        private void Update()
        {
            moveDirection = inputProvider.GetAxis();
            ClampPosition();
        }

        private void ClampPosition()
        {
            bool clamped = false;
            var position = transform.position;
            if (position.x <= -xExtents)
            {
                clamped = true;
                position.x = -xExtents;
                moveDirection = Mathf.Max(0, moveDirection);
            }
            else if (position.x >= xExtents)
            {
                clamped = true;
                position.x = xExtents;
                moveDirection = Mathf.Min(0, moveDirection);
            }
            if (clamped)
                transform.position = position;
        }

        private void FixedUpdate()
        {
            float movement = moveDirection * moveSpeed;
            Rigidbody.velocity = new Vector2(movement, 0);
        }

        private void OnDisable()
        {
            Rigidbody.velocity = Vector2.zero;
        }
    }
}
