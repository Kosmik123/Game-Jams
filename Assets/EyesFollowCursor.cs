using UnityEngine;

public static class GizmosHelper
{
	public static void DrawCone(Vector3 apexPosition, Vector3 direction, float range, float angle)
	{
		if (direction.sqrMagnitude > 1)
			direction.Normalize();

		float angleRadians = angle * Mathf.Deg2Rad;

		float height = range * Mathf.Cos(angleRadians);
		Vector3 baseCenter = apexPosition + direction * height;

		float baseRadius = range * Mathf.Sin(angleRadians);
#if UNITY_EDITOR
		var previousColor = UnityEditor.Handles.color;
		UnityEditor.Handles.color = Gizmos.color;
		UnityEditor.Handles.DrawWireDisc(baseCenter, direction, baseRadius);
		UnityEditor.Handles.color = previousColor;
#endif

		Vector3 radiusDirection = Vector3.Cross(direction, Vector3.up);
		if (radiusDirection.sqrMagnitude < 0.001f)
			radiusDirection = Vector3.Cross(direction, Vector3.forward);

		radiusDirection.Normalize();
		for (int i = 0; i < 4; i++)
		{
			var lineEnd = baseCenter + radiusDirection * baseRadius;
			Gizmos.DrawLine(apexPosition, lineEnd);
			radiusDirection = Quaternion.AngleAxis(90, direction) * radiusDirection;
		}
	}
}

public class EyesFollowCursor : MonoBehaviour
{
	[SerializeField]

	private ConeLookAtLimit[] eyes;

	[SerializeField, Range(0, 1)]
	private float lookedDistanceMultiplier = 0.5f;

	private Camera viewCamera;
	private Vector3 cursorWorldPosition;

	private void Awake()
	{
		viewCamera = Camera.main;
	}

	private void Update()
	{
		var distanceFromCamera = (viewCamera.transform.position - transform.position).magnitude * lookedDistanceMultiplier;
		var cursorScreenPosition = Input.mousePosition;
		cursorScreenPosition.z = distanceFromCamera;

		cursorWorldPosition = viewCamera.ScreenToWorldPoint(cursorScreenPosition);
		foreach (var eye in eyes)
		{
			eye.LookAt(cursorWorldPosition);
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.cyan;
		Gizmos.DrawSphere(cursorWorldPosition, 0.1f);
	}
}
