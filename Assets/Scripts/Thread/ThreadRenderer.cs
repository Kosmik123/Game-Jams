using UnityEngine;

public class ThreadRenderer : MonoBehaviour
{
    [SerializeField]
    private LineRenderer lineRenderer;
    [SerializeField]
    private ThreadPhysics threadPhysics;


    private void Update()
    {
        var middlePoints = threadPhysics.Points;
        lineRenderer.positionCount = middlePoints.Count + 1;
        for (int i = 0; i < middlePoints.Count; i++) 
            lineRenderer.SetPosition(i, middlePoints[i]);

        lineRenderer.SetPosition(middlePoints.Count, threadPhysics.transform.position);
    }

}
