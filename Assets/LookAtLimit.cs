using UnityEngine;

public abstract class LookAtLimit : MonoBehaviour
{
	[SerializeField]
	protected Transform rotatedTransform;

	public abstract Vector3 GetLimitedDirection(Vector3 direction);

	public Vector3 GetLimitedDirectionFromTarget(Vector3 target)
	{
		Vector3 direction = target - rotatedTransform.position;
		return GetLimitedDirection(direction);
	}

	public void LookAt(Vector3 target)
	{
		rotatedTransform.forward = GetLimitedDirectionFromTarget(target);
	}
}
