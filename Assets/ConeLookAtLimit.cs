using UnityEngine;

public class ConeLookAtLimit : LookAtLimit
{
	[SerializeField]
	private float rangeConeAngle;

	public override Vector3 GetLimitedDirection(Vector3 direction)
	{
		Vector3 rotationAxis = Vector3.Cross(direction, transform.forward);
		float angle = Vector3.SignedAngle(transform.forward, direction, rotationAxis);
		if (Mathf.Abs(angle) > rangeConeAngle)
			direction = Quaternion.AngleAxis(rangeConeAngle * Mathf.Sign(angle), rotationAxis) * transform.forward;

		return direction;
	}

	private void OnDrawGizmosSelected()
	{
		if (rotatedTransform)
		{
			Gizmos.color = Color.yellow;
			GizmosHelper.DrawCone(rotatedTransform.position, transform.forward, 0.5f, rangeConeAngle);
		}
	}
}
