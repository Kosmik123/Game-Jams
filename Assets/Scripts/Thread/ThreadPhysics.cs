using System.Collections.Generic;
using UnityEngine;

public class ThreadPhysics : MonoBehaviour
{
    private readonly List<Vector3> points = new List<Vector3>();
    public IReadOnlyList<Vector3> Points => points;

    [SerializeField]
    private Transform origin;
    public Transform Origin
    {
        get => origin;
        set
        {
            origin = value;
        }
    }

    [SerializeField]
    private Transform ending;
    public Transform Ending
    {
        get => ending;
        set
        {
            ending = value;
        }
    }

    [SerializeField, Min(0.01f)]
    private float thickness = 0.1f;
    public float Thickness
    {
        get => thickness;
        set => thickness = value;
    }

    [SerializeField]
    private LayerMask detectedLayers = -5;

    [SerializeField, NaughtyAttributes.ReadOnly]
    private float length;
    private bool isLengthCalculated;
    public float Length
    {
        get
        {
            if (isLengthCalculated == false)
                CalculateLength();
            return length;
        }
    }

    private void OnEnable()
    {
        PopulateList();
    }

    private void PopulateList()
    {
        if (origin == null || ending == null)
        {
            enabled = false;
            return;
        }

        points.Clear();
        points.Add(origin.position);
        points.Add(ending.position);
    }

    private const float preferredChecksDistance = 0.1f;

    private void FixedUpdate()
    {
        int lastIndex = points.Count - 1;
        DetectCollisionEnter(lastIndex, lastIndex - 1, ending);
        DetectCollisionEnter(0, 1, origin);

        DetectCollisionExit(points.Count - 1, -1);
        DetectCollisionExit(0, +1);
        isLengthCalculated = false;

#if UNITY_EDITOR
        CalculateLength();
#endif
    }

    private void CalculateLength()
    {
        float distanceSum = 0;
        for (int i = 1; i < points.Count; i++)
        {
            var start = points[i - 1];
            var end = points[i];
            distanceSum += (start - end).magnitude;
        }
        length = distanceSum;
        isLengthCalculated = true;
    }

    private void DetectCollisionEnter(int tipPointIndex, int neighbourPointIndex, Transform tipTransform)
    {
        var previousPosition = points[tipPointIndex];
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

                var hitCenter = (hitPoint1 + hitPoint2) / 2f;
                var hitNormal = (hitNormal1 + hitNormal2) / 2f;
                if (hitNormal.sqrMagnitude < 0.001f)
                    hitNormal = reversedPositionDelta;
                hitNormal.Normalize();

                bool wasSafeHit = false;
                for (int i = 1; i < 10; i++)
                {
                    var safePointDetectionLineStart = hitCenter + i * thickness * hitNormal;
                    if (Physics.Linecast(safePointDetectionLineStart, hitCenter, out var safePointDetectionHit, detectedLayers) == false)
                        continue;

                    var safePoint = safePointDetectionHit.point + safePointDetectionHit.normal * thickness;
                    points.Insert(Mathf.Max(neighbourPointIndex, tipPointIndex), safePoint);
                    wasSafeHit = true;
                    break;
                }

                if (wasSafeHit == false)
                    Debug.LogError("NIEMO¯LIWE");

                break;
            }
        }
    }

    private void DetectCollisionExit(int tipIndex, int direction)
    {
        if (points.Count > 2)
        {
            var secondNeighbour = points[tipIndex + 2 * direction];
            var tipPoint = points[tipIndex];

            if (DoubleLinecast(tipPoint, secondNeighbour, out _, out _, detectedLayers) == false)
            {
                float distance = (tipPoint - secondNeighbour).magnitude;
                var hypotenuseRay = new Ray(tipPoint, secondNeighbour - tipPoint);
                int triangleChecksResolution = 1 + Mathf.CeilToInt(distance * 5);
                float rayStartBaseDistance = distance / triangleChecksResolution;
                int neighbourIndex = tipIndex + direction;
                var neighbourPoint = points[neighbourIndex];
                for (int i = 1; i <= triangleChecksResolution; i++)
                {
                    Debug.DrawLine(tipPoint, secondNeighbour, new Color(0.1f, 0.1f, 0.3f, 0.3f), 1f);
                    var lineEnd = hypotenuseRay.GetPoint(rayStartBaseDistance * i);
                    if (Physics.Linecast(neighbourPoint, lineEnd, detectedLayers))
                        return;
                }

                points.RemoveAt(neighbourIndex);
            }
        }
    }

    private static bool DoubleLinecast(Vector3 point1, Vector3 point2, out RaycastHit fromPoint1Info, out RaycastHit fromPoint2Info, LayerMask layerMask)
    {
        bool fromPoint1 = Physics.Linecast(point1, point2, out fromPoint1Info, layerMask);
        bool fromPoint2 = Physics.Linecast(point2, point1, out fromPoint2Info, layerMask);
        bool wasHit = fromPoint1 || fromPoint2;
        return wasHit;
    }

    private void OnDrawGizmos()
    {
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
