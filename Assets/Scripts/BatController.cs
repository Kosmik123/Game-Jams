using UnityEngine;
using Bipolar.Input;

namespace Bipolar.Pong
{
	[RequireComponent(typeof(Rigidbody2D))]
	public class BatController : MonoBehaviour
	{
		private Rigidbody2D rigidbody2d;

		[SerializeField]
		private AxisInputProvider inputProvider;

		[SerializeField]
		private float moveSpeed;

		[SerializeField]
		private float batExtents = 2;

		private float input;

		private void Awake()
		{
			rigidbody2d = GetComponent<Rigidbody2D>();
			rigidbody2d.isKinematic = true;
		}

		private void Update()
		{
			input = inputProvider.GetAxis();
		}

		private void FixedUpdate()
		{
			var position = rigidbody2d.position;
			var movement = input * moveSpeed * Time.deltaTime;
			position.y += movement;
			position.y = Mathf.Clamp(position.y, -SceneSettings.SceneBorder + batExtents, SceneSettings.SceneBorder - batExtents);

			rigidbody2d.position = position;
		}
	}
}
