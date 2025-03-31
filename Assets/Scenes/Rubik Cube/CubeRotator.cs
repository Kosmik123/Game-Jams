using System.Collections;
using UnityEngine;

public class CubeRotator : MonoBehaviour
{
	public event System.Action OnSnapped;

	public enum AxisIndex
	{
		None = -1,
		X = 0,
		Y = 1,
		Z = 2
	}

	[SerializeField]
	private RubikCube rubikCube;
	public RubikCube RubikCube => rubikCube;
	[SerializeField]
	private Transform rotationHelper;
	[SerializeField]
	private LayerMask cubeLayers;
	[SerializeField]
	private float rotationAngleThreshold;
	[SerializeField]
	private float autoSnappingRotationSpeed = 300;
	[SerializeField]
	private float autoSnapAngleThreshold = 20f;

	[Header("States")]
	[SerializeField]
	private Vector3 pressPointLocalPosition;
	[SerializeField]
	private float pressPointDistance;

	[SerializeField]
	private Vector3 currentPointLocalPosition;

	[SerializeField]
	private AxisIndex horizontalRotationAxis = AxisIndex.None;
	[SerializeField]
	private AxisIndex verticalRotationAxis = AxisIndex.None;

	[SerializeField]
	private float horizontalAngle;
	[SerializeField]
	private float verticalAngle;

	[Space]
	[SerializeField]
	private Vector3 locallyVerticalAxisForHorizontalSwipe;
	[SerializeField]
	private Vector3 locallyHorizontalAxisForVerticalSwipe;
	[SerializeField]
	private Vector3Int pressedSubcubeIndex;
	[SerializeField]
	private AxisIndex currentlyRotatedAxisIndex = AxisIndex.None;

	[Header("Snapping States")]
	[SerializeField]
	private bool isSnapping;

	private Camera viewCamera;
	private bool isDragging;

	private void Awake()
	{
		var dimensions = rubikCube.Dimensions;
		viewCamera = Camera.main;
		ResetState();
	}

	private void ResetState()
	{
		horizontalRotationAxis = AxisIndex.None;
		verticalRotationAxis = AxisIndex.None;
		currentlyRotatedAxisIndex = AxisIndex.None;

		currentPointLocalPosition = Vector3.zero;
		pressPointLocalPosition = Vector3.zero;
		pressPointDistance = 0;
		verticalAngle = 0;
		horizontalAngle = 0;
	}

	private void Update()
	{
		bool wasDragging = isDragging;
		isDragging &= Input.GetMouseButton(0);
	
		if (isSnapping)
			return;

		if (isDragging == false && Input.GetMouseButtonDown(0))
		{
			var cursorScreenPosition = Input.mousePosition;
			var ray = viewCamera.ScreenPointToRay(cursorScreenPosition);
			if (Physics.Raycast(ray, out var hit, float.PositiveInfinity, cubeLayers))
			{
				isDragging = true;
				ResetHelperRotation();
				pressPointDistance = hit.distance;

				cursorScreenPosition.z = pressPointDistance;
				var cursorWorldPosition = viewCamera.ScreenToWorldPoint(cursorScreenPosition);
				pressPointLocalPosition = transform.InverseTransformPoint(cursorWorldPosition);

				pressedSubcubeIndex = rubikCube.PositionToIndex(pressPointLocalPosition);

				horizontalRotationAxis = GetBestProjection(viewCamera.transform.up);
				verticalRotationAxis = GetBestProjection(viewCamera.transform.right);
			}
		}

		if (isDragging)
		{
			HandleDragging();
		}
		else if (wasDragging)
		{

			(float angle, Vector3 axis) = GetCurrentAngleAndAxis();
			StartCoroutine(SnapRotatingSliceCo(angle, axis));
			ResetState();
		}
	}

	private void HandleDragging()
	{
		var cursorScreenPosition = Input.mousePosition;
		cursorScreenPosition.z = pressPointDistance;
		var cursorWorldPosition = viewCamera.ScreenToWorldPoint(cursorScreenPosition);
		currentPointLocalPosition = transform.InverseTransformPoint(cursorWorldPosition);

		locallyVerticalAxisForHorizontalSwipe = GetVector(horizontalRotationAxis);
		horizontalAngle = AngleAroundAxis(pressPointLocalPosition, currentPointLocalPosition, locallyVerticalAxisForHorizontalSwipe);

		locallyHorizontalAxisForVerticalSwipe = GetVector(verticalRotationAxis);
		verticalAngle = AngleAroundAxis(pressPointLocalPosition, currentPointLocalPosition, locallyHorizontalAxisForVerticalSwipe);

		float verticalAngleValue = Mathf.Abs(verticalAngle);
		float horizontalAngleValue = Mathf.Abs(horizontalAngle);

		if (currentlyRotatedAxisIndex == AxisIndex.None)
		{
			if (verticalAngleValue > horizontalAngleValue)
				currentlyRotatedAxisIndex = verticalRotationAxis;
			else if (horizontalAngleValue > verticalAngleValue)
				currentlyRotatedAxisIndex = horizontalRotationAxis;
		}
		else
		{
			ResetHelperRotation();
			rubikCube.ResetParents();

			(float angle, Vector3 currentRotationAxis) = GetCurrentAngleAndAxis();

			foreach (var subcube in rubikCube.GetSubcubesSlice(pressedSubcubeIndex, (int)currentlyRotatedAxisIndex))
			{
				subcube.transform.parent = rotationHelper;
			}
			rotationHelper.localRotation = Quaternion.AngleAxis(angle, currentRotationAxis);

			if (angle > autoSnapAngleThreshold)
			{
				StartCoroutine(SnapRotatingSliceCo(angle, currentRotationAxis, Mathf.Sign(angle)));
			}
		}
	}

	private (float angle, Vector3 axis) GetCurrentAngleAndAxis()
	{
		var axis = Vector3.zero;
		float angle = 0;
		if (currentlyRotatedAxisIndex == verticalRotationAxis)
		{
			angle = verticalAngle;
			axis = locallyHorizontalAxisForVerticalSwipe;
		}
		else if (currentlyRotatedAxisIndex == horizontalRotationAxis)
		{
			angle = horizontalAngle;
			axis = locallyVerticalAxisForHorizontalSwipe;
		}

		return (angle, axis);
	}

	private IEnumerator SnapRotatingSliceCo(float angle, Vector3 axis, float direction = 0)
	{
		isSnapping = true;

		if (direction == 0)
			direction = Mathf.RoundToInt(angle / 90f); 

		float targetAngle = 90 * direction;
		float currentAngle = angle;
		while (currentAngle != targetAngle)
		{
			currentAngle = Mathf.MoveTowards(currentAngle, targetAngle, autoSnappingRotationSpeed * Time.deltaTime );
			rotationHelper.localRotation = Quaternion.AngleAxis(currentAngle, axis);
			yield return null;
		}
		rotationHelper.localRotation = Quaternion.AngleAxis(targetAngle, axis);
		rubikCube.ResetParents();
		ResetHelperRotation();
		rubikCube.RecalculateIndices();
		isSnapping = false;
		OnSnapped?.Invoke();
	}

	private void ResetHelperRotation()
	{
		rotationHelper.localRotation = Quaternion.identity;
	}

	private static float AngleAroundAxis(Vector3 from, Vector3 to, Vector3 axis)
	{
		var projectedFrom = Vector3.ProjectOnPlane(from, axis);
		var projectedTo = Vector3.ProjectOnPlane(to, axis);
		return Vector3.SignedAngle(projectedFrom, projectedTo, axis);
	}

	private AxisIndex GetBestProjection(Vector3 worldAxis)
	{
		int bestProjectionAxis = 0;
		float bestDot = 0;
		for (int i = 0; i < 3; i++)
		{
			var axis = GetVector(i);
			var dot = Mathf.Abs(Vector3.Dot(transform.TransformDirection(axis), worldAxis));
			if (dot > bestDot)
			{
				bestDot = dot;
				bestProjectionAxis = i;
			}
		}

		return (AxisIndex)bestProjectionAxis;
	}

	private Vector3 GetVector(AxisIndex axis) => GetVector((int)axis);
	private Vector3 GetVector(int axisIndex)
	{
		var vector = Vector3.zero;
		vector[axisIndex] = 1f;
		return vector;
	}
}

public static class RoundingHelper
{
	public static float RoundToNearestMultiple(float number, float multipleOf)
	{
		int count = Mathf.RoundToInt(number / multipleOf);
		return count * multipleOf;
	}
}