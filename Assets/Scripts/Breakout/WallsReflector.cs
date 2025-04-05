using Bipolar.Breakout;
using UnityEngine;

public class WallsReflector : MonoBehaviour, IBallReflector
{
	[SerializeField, Range(0, 1)]
	private float modificationValue;

	public Vector2 GetBounceDirection(IBall ball, Collision2D collision)
	{
		var hitNormal = collision.GetContact(0).normal;
		var ballVelocity = ball.Velocity;
		var defaultReflectedDirection = Vector3.Reflect(ballVelocity, hitNormal).normalized;

		var verticalVelocity = ballVelocity;
		verticalVelocity.x = 0;

		var modifiedReflectedDirection = (Vector2)defaultReflectedDirection + modificationValue * verticalVelocity.normalized;
		return modifiedReflectedDirection.normalized; 
	}
}
