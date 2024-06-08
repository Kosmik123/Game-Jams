using UnityEngine;

[System.Serializable]
public struct PathOrientation
{
    public Vector3 position;
    public float rotation;
    public bool useRotation;

    public PathOrientation(Vector3 position, float rotation, bool useRotation = true)
    {
        this.position = position;
        this.rotation = rotation;
        this.useRotation = useRotation;
    }
}

public class NPCMovement : MonoBehaviour
{
    [SerializeField]
    private Transform movedTransform;
    [SerializeField]
    private PathOrientation[] pathPoints;

    [SerializeField]
    private float moveSpeed = 4;
    [SerializeField]
    private float rotationSpeed = 200;

    [field: SerializeField]
    public bool IsInversed { get; set; }

    private bool isMoving;
    public bool IsMoving => isMoving;

    private int targetPointIndex;

    [ContextMenu("Start")]
    public void StartMoving()
    {
        targetPointIndex = IsInversed ? pathPoints.Length - 1 : 0;
        initialPosition = movedTransform.position;
        isMoving = true;
    }

    [ContextMenu("Stop")]
    public void StopMoving()
    {
        isMoving = false;
    }

    private void Update()
    {
        if (isMoving == false)
            return;

        float dt = Time.deltaTime;
        float maxDistance = moveSpeed * dt;
        float maxRotate = rotationSpeed * dt;
        PathOrientation target = pathPoints[targetPointIndex];
        float currentAngle = movedTransform.rotation.eulerAngles.y;
        Vector3 direction = target.position - movedTransform.position;

        float targetAngle = target.useRotation ? target.rotation : Vector3.SignedAngle(Vector3.forward, direction, Vector3.up);
        currentAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, maxRotate);
        Vector3 currentPosition = Vector3.MoveTowards(movedTransform.position, target.position, maxDistance);

        movedTransform.SetPositionAndRotation(currentPosition, Quaternion.AngleAxis(currentAngle, Vector3.up));
        if ((currentPosition - target.position).sqrMagnitude < maxDistance * maxDistance && Mathf.Abs(targetAngle - currentAngle) < maxRotate)
        {
            movedTransform.SetPositionAndRotation(target.position, Quaternion.AngleAxis(targetAngle, Vector3.up));
            targetPointIndex += IsInversed ? -1 : 1;
            if (targetPointIndex >= pathPoints.Length || targetPointIndex < 0)
                isMoving = false;
        }
    }

    private Vector3 initialPosition;
    private void OnDrawGizmosSelected()
    {
        if (pathPoints == null || pathPoints.Length == 0)
            return;

        Color color = Color.yellow;
        color.a = 0.33f;
        Gizmos.color = color;
        var previousPoint = isMoving ? initialPosition : movedTransform ? movedTransform.position : pathPoints[0].position;
        for (int i = 0; i < pathPoints.Length; i++)
        {
            int index = IsInversed ? pathPoints.Length - 1 - i : i;
            var point = pathPoints[index];
            Gizmos.DrawLine(point.position, previousPoint);
            previousPoint = point.position;
            Gizmos.color = Color.yellow;
        }
    }
}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(NPCMovement))]
public class NPCMovementEditor : UnityEditor.Editor
{
    private void OnSceneGUI()
    {
        var pointsProperty = serializedObject.FindProperty("pathPoints");
        for (int i = 0; i < pointsProperty.arraySize; i++)
        {
            var pointProperty = pointsProperty.GetArrayElementAtIndex(i);
            var orientation = GetPathOrientation(pointProperty);

            bool isUsingRotation = pointProperty.FindPropertyRelative("useRotation").boolValue;
            if (isUsingRotation)
            {
                Quaternion rotation = Quaternion.AngleAxis(orientation.rotation, Vector3.up);
                UnityEditor.Handles.TransformHandle(ref orientation.position, ref rotation);
                orientation.rotation = rotation.eulerAngles.y;
            }
            else
            {
                orientation.position = UnityEditor.Handles.PositionHandle(orientation.position, Quaternion.identity);
            }
            SetPathOrientation(pointProperty, orientation);
        }
        pointsProperty.serializedObject.ApplyModifiedProperties();
    }

    private void SetPathOrientation(UnityEditor.SerializedProperty property, PathOrientation pathOrientation)
    {
        property.FindPropertyRelative("position").vector3Value = pathOrientation.position;
        property.FindPropertyRelative("rotation").floatValue = pathOrientation.rotation;
    }

    private PathOrientation GetPathOrientation(UnityEditor.SerializedProperty property)
    {
        Vector3 position = property.FindPropertyRelative("position").vector3Value;
        float rotation = property.FindPropertyRelative("rotation").floatValue;
        return new PathOrientation(position, rotation);
    }
}
#endif
