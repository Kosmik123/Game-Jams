using UnityEngine;
using Bipolar.Input;
using Mono.Cecil.Cil;

namespace Bipolar.Breakout
{
	[RequireComponent(typeof(Rigidbody2D))]
	public class PaddleMovement : MonoBehaviour
	{
		[SerializeField, Min(0)]
		private float xExtents = 10;

		private void Update()
		{
			var mouseScreenPosition = Camera.main.ScreenToWorldPoint(UnityEngine.Input.mousePosition);
			var position = transform.position;
			position.x = mouseScreenPosition.x;
			transform.position = position;
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
			}
			else if (position.x >= xExtents)
			{
				clamped = true;
				position.x = xExtents;
			}
			if (clamped)
				transform.position = position;
		}
	}
}
