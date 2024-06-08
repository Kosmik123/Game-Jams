using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;

public class ThreadPhysics : MonoBehaviour
{
    [SerializeField, ReadOnly]
    private List<Vector3> points;
    public IReadOnlyList<Vector3> Points => points;

    [SerializeField]
    private Transform origin;

    [SerializeField, Min(0.01f)]
    private float thickness = 0.1f;

    [SerializeField]
    private LayerMask detectedLayers = -5;

    public Vector3 hitCenter;
    public Vector3 hitNormal;
    public Vector3 safePoint;

    private Vector3 previousPosition;
    private Vector3 previousOriginPosition;

    private void Awake()
    {
        points.Clear();
        points.Add(origin.position);
        points.Add(transform.position);
    }

    private const float preferredChecksDistance = 0.05f;
    
    private void Update()
    {
        int lastIndex = points.Count - 1;
        DetectCollisionEnter(lastIndex, lastIndex - 1, transform);
        DetectCollisionEnter(0, 1, origin);

        //DetectCollisionExit();
    }

    private void DetectCollisionEnter(int tipPointIndex, int neighbourPointIndex, Transform tipTransform)
    {
        previousPosition = points[tipPointIndex];
        points[tipPointIndex] = tipTransform.position;

        var previousTipPosition = previousPosition;
        var currentTipPosition = points[tipPointIndex];

        if (currentTipPosition == previousTipPosition)
            return;
        
        Vector3 reversedPositionDelta = previousTipPosition - currentTipPosition;
        var reversedPositionDeltaRay = new Ray(currentTipPosition, reversedPositionDelta);
        float movedDistance = reversedPositionDelta.magnitude;
        int checksResolution = 1 + Mathf.Max(1, Mathf.CeilToInt(movedDistance / preferredChecksDistance));
        float checkBaseDistance = movedDistance / checksResolution;

        var neighbourPosition = points[neighbourPointIndex];
        for (int j = checksResolution - 1; j >= 0; j--)
        {
            var checkedPoint = reversedPositionDeltaRay.GetPoint(j * checkBaseDistance);
            if (DoubleLinecast(checkedPoint, neighbourPosition, out var hit1, out var hit2, detectedLayers))
            {
                bool hit1Valid = hit1.collider;
                bool hit2Valid = hit2.collider;

                var hitPoint1 = hit1Valid ? hit1.point : hit2.point;
                var hitPoint2 = hit2Valid ? hit2.point : hit1.point;

                var hitNormal1 = hit1Valid ? hit1.normal : hit2.normal;
                var hitNormal2 = hit2Valid ? hit2.normal : hit1.normal;

                hitCenter = (hitPoint1 + hitPoint2) / 2f;
                hitNormal = (hitNormal1 + hitNormal2) / 2f;
                if (hitNormal.sqrMagnitude < 0.001f)
                    hitNormal = reversedPositionDelta;
                hitNormal.Normalize();

                bool wasSafeHit = false;
                for (int i = 1; i < 10; i++)
                {
                    var safePointDetectionLineStart = hitCenter + i * thickness * hitNormal;
                    if (Physics.Linecast(safePointDetectionLineStart, hitCenter, out var safePointDetectionHit, detectedLayers) == false)
                        continue;

                    safePoint = safePointDetectionHit.point + safePointDetectionHit.normal * thickness;
                    points.Insert(Mathf.Max(neighbourPointIndex, tipPointIndex), safePoint);
                    wasSafeHit = true;
                    break;
                }

                if (wasSafeHit == false)
                    Debug.LogError("NIEMO¯LIWE RACZEJ W METODZIE");

                break;
            }
        }
    }

    private void DetectCollisionExit()
    {
        if (points.Count <= 1)
            return;

        var preLastPoint = points[points.Count - 2];
        if (DoubleLinecast(transform.position, preLastPoint, out var hitFromOrigin, out var hitToOrigin, Physics.DefaultRaycastLayers) == false)
        {
            float distance = (transform.position - preLastPoint).magnitude;
            var hypotenuseRay = new Ray(transform.position, preLastPoint - transform.position);
            int triangleCheckRaysCount = Mathf.CeilToInt(distance * 5 + 1);
            float rayStartsDistance = distance / triangleCheckRaysCount;
            for (int i = 0; i <= triangleCheckRaysCount; i++)
            {
                var lineEnd = hypotenuseRay.GetPoint(rayStartsDistance * i);
                if (Physics.Linecast(points[points.Count - 1], lineEnd))
                    return;
            }

            for (int i = 0; i <= triangleCheckRaysCount; i++)
            {
                var lineEnd = hypotenuseRay.GetPoint(rayStartsDistance * i);
                Debug.DrawLine(points[points.Count - 1], lineEnd, Color.green, 2f);
            }

            points.RemoveAt(points.Count - 1);
        }
    }

    private static bool DoubleLinecast(Vector3 point1, Vector3 point2, out RaycastHit fromPoint1Info, out RaycastHit fromPoint2Info, LayerMask layerMask)
    {
        bool fromPoint1 = Physics.Linecast(point1, point2, out fromPoint1Info, layerMask);
        bool fromPoint2 = Physics.Linecast(point2, point1, out fromPoint2Info, layerMask);
        return fromPoint1 || fromPoint2;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(hitCenter, 0.05f);

        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(safePoint, 0.05f);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(hitCenter, hitCenter + hitNormal);

        if (points == null || points.Count <= 0)
            return;

        Gizmos.color = Color.yellow;
        if (points.Count > 1)
        {
            for (int i = 1; i < points.Count; i++)
            {
                var start = points[i - 1];
                var end = points[i];
                Gizmos.DrawLine(start, end);
            }
        }
    }
}
