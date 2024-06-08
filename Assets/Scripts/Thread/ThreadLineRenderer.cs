using UnityEngine;

public class ThreadLineRenderer : MonoBehaviour
{
    [SerializeField]
    private LineRenderer lineRenderer;
    [SerializeField]
    private ThreadPhysics threadPhysics;

    private void Update()
    {
        var points = threadPhysics.Points;
        lineRenderer.positionCount = points.Count;
        for (int i = 0; i < points.Count; i++) 
            lineRenderer.SetPosition(i, points[i]);
    }
}
