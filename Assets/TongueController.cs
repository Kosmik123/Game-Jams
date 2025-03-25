using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class TongueController : MonoBehaviour
{
	[SerializeField]
	private SplineContainer detectionSplineContainer;
	[SerializeField]
	private SplineContainer renderedSplineContainer;
	[SerializeField]
	private Transform target;

	[SerializeField]
	private float endingTangentLength = 0.1f;
	[SerializeField]
	private float tongueThickness = 0.1f;

	[SerializeField]
	private LayerMask playerLayers;
	[SerializeField]
	private int playerCollisionChecksCount = 5;
	[SerializeField]
	private float distanceFromBody = 0.5f;

	private Camera viewCamera;
	private Spline renderedSpline;
	private Spline detectionSpline;

	private void Awake()
	{
		viewCamera = Camera.main;
	}

	private void OnEnable()
	{
		renderedSpline = renderedSplineContainer.Spline;
		detectionSpline = detectionSplineContainer.Spline;
	}

	private void Update()
	{
		gizmosPoints.Clear();
		gizmosCorrectPoints.Clear();
		renderedSpline.Clear();

		var ray = viewCamera.ScreenPointToRay(Input.mousePosition);
		//if (Physics.Raycast(ray, out var hit, 100f, detectedLayers))
		{
			var targetPosition = target.position;
			var targetNormal = target.up;

			var targetLocalPosition = transform.InverseTransformPoint(targetPosition);
			var targetLocalNormal = transform.InverseTransformDirection(targetNormal);

			var tongueStartPosition = transform.TransformPoint(detectionSpline[0].Position);
			var directionFromStart = targetPosition - tongueStartPosition;
			Debug.DrawRay(tongueStartPosition, directionFromStart, Color.black);
			
			var rotationAxis = Vector3.Cross(directionFromStart, targetNormal);
			Debug.DrawRay(targetPosition, rotationAxis, Color.blue);

			var tangent = Vector3.Cross(targetNormal, rotationAxis).normalized * endingTangentLength;
			Debug.DrawRay(targetPosition, tangent, Color.red);

			var localTangent = transform.InverseTransformDirection(tangent);

			var targetKnot = new BezierKnot(targetLocalPosition + targetLocalNormal * tongueThickness, -localTangent, localTangent);
			detectionSpline.SetKnot(detectionSpline.Count - 1, targetKnot);
		}

		var tongueStart = transform.TransformPoint(detectionSpline[0].Position);
		var tongueEnd = transform.TransformPoint(detectionSpline[^1].Position);

		var curveCenter = (tongueStart + tongueEnd) / 2f;

		float step = 1f / (playerCollisionChecksCount + 1);

		renderedSpline.Add(detectionSpline[0]);

		for (float i = 0; i < playerCollisionChecksCount; i++)
		{
			float t = (i + 1) * step;
			var localPoint = detectionSpline.EvaluatePosition(t);
			var point = transform.TransformPoint(localPoint);

			var outsideDirection = (point - curveCenter).normalized;
			var rayStart = point + outsideDirection;
			if (Physics.Linecast(rayStart, curveCenter, out var bodyHit, playerLayers))
			{
				Debug.DrawLine(rayStart, curveCenter, Color.red);
				var correctPoint = bodyHit.point + outsideDirection * distanceFromBody;
				gizmosCorrectPoints.Add(correctPoint);

				var tangent = detectionSpline.EvaluateTangent(t) * 0.01f;
				renderedSpline.Add(new BezierKnot(
					transform.InverseTransformPoint(correctPoint), 
					-tangent, 
					tangent));
			}
			else
			{
				Debug.DrawLine(rayStart, curveCenter, Color.green);
			}

			gizmosPoints.Add(localPoint);
		}

		renderedSpline.Add(detectionSpline[^1]);
	}

	private readonly List<float3> gizmosPoints = new List<float3>();
	private readonly List<float3> gizmosCorrectPoints = new List<float3>();

	private void OnDrawGizmosSelected()
	{
		for (int i = 0; i < gizmosPoints.Count; i++)
		{
			Gizmos.color = Color.red;
			float3 point = transform.TransformPoint(gizmosPoints[i]);
			Gizmos.DrawSphere(point, 0.05f);
		}

		for (int i = 0; i < gizmosCorrectPoints.Count; i++)
		{
			Gizmos.color = Color.green;
			float3 point = gizmosCorrectPoints[i];
			Gizmos.DrawSphere(point, 0.05f);
		}

	}

	private bool AvoidCollisionsWithBody()
	{
		bool collidedWithBody = false;
		for (int i = 1; i < renderedSpline.Count - 1; i++)
		{
			var knot1 = renderedSpline[i - 1];
			var knot2 = renderedSpline[i];

			var position1 = transform.TransformPoint(knot1.Position);
			var position2 = transform.TransformPoint(knot2.Position);

			if (Physics.Linecast(position1, position2, playerLayers) == false)
				continue;

			collidedWithBody = true;
			var segmentNormalLocal = (knot1.TangentOut + knot2.TangentIn) / 2;
			var lineCenter = (position1 + position2) / 2;
			var segmentNormal = transform.TransformDirection(segmentNormalLocal).normalized;
			var ray = new Ray(lineCenter + segmentNormal, -segmentNormal);
			if (Physics.Raycast(ray, out var hit, 1f, playerLayers) == false)
			{
				Debug.LogError("!");
				continue;
			}

			var segmentTangent = (position2 - position1).normalized;
			var localSegmentTangent = transform.InverseTransformDirection(segmentTangent);
			var localMiddlePoint = transform.InverseTransformPoint(hit.point + segmentNormal * tongueThickness);

			var middleKnot = new BezierKnot(
				localMiddlePoint,
				-localSegmentTangent,
				localSegmentTangent);


		}



		return collidedWithBody;
	}
}
