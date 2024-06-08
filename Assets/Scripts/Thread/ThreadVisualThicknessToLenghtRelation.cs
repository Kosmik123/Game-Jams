using UnityEngine;

public class ThreadVisualThicknessToLenghtRelation : MonoBehaviour
{
    [SerializeField]
    private ThreadPhysics thread;
    [SerializeField]
    private LineRenderer threadRenderer;
    [SerializeField]
    private ThreadLengthConstraint lengthConstraint;

    [SerializeField]
    private AnimationCurve thicknessCurve;
    [SerializeField]
    private float thicknessMultiplier;

    private void Update()
    {
        float relativeLength = thread.Length / lengthConstraint.MaxLength;
        float thickness = thicknessMultiplier * thicknessCurve.Evaluate(relativeLength);
        threadRenderer.widthMultiplier = thickness;
    }
}