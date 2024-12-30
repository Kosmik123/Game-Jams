using NaughtyAttributes;
using UnityEngine;

namespace Bipolar.Pong
{

	[RequireComponent(typeof(Rigidbody2D))]
	public class BallController : MonoBehaviour
	{
		public event System.Action<int> OnBorderExceeded;

		private Rigidbody2D rigidbody2d;

		[SerializeField]
		private float startingSpeed;
		[SerializeField]
		private float radius;
		public float Radius => radius;

		[SerializeField, Tag]
		private string batTag;
		[SerializeField]
		private float accelerationPerHit = 1;

		[Header("States")]
		[SerializeField]
		private float currentSpeed;
		
		private void Awake()
		{
			rigidbody2d = GetComponent<Rigidbody2D>();
		}

		public void StartMoving(Vector2 direction)
		{
			currentSpeed = startingSpeed;
			rigidbody2d.velocity = direction.normalized * currentSpeed;
		}

		private void FixedUpdate()
		{
			var velocity = rigidbody2d.velocity;
			var position = rigidbody2d.position;
			if (position.y > SceneSettings.SceneBorder - radius)
			{
				velocity.y = -Mathf.Abs(velocity.y);
				velocity.x *= 1.1f;
			}
			else if (position.y < -SceneSettings.SceneBorder + radius)
			{
				velocity.y = Mathf.Abs(velocity.y);
				velocity.x *= 1.1f;
			}

			if (position.x > SceneSettings.RightBorder)
			{
				OnBorderExceeded.Invoke(+1);
			}
			else if (position.x < SceneSettings.LeftBorder)
			{
				OnBorderExceeded.Invoke(-1);
			}
			
			rigidbody2d.velocity = velocity;
		}

		private void OnTriggerEnter2D(Collider2D collision)
		{
			if (collision.CompareTag(batTag))
			{
				var bat = collision.transform;
				var batPosition = bat.position;
				var ballPositon = transform.position;
				var newDirection = (ballPositon - batPosition).normalized;
				float angle = Vector2.SignedAngle(bat.up, newDirection);
				angle *= 0.8f;
				newDirection = Quaternion.AngleAxis(angle, Vector3.forward) * bat.up;

				currentSpeed += accelerationPerHit;
				rigidbody2d.velocity = newDirection * currentSpeed;
			}
		}
	}
}