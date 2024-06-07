using UnityEngine;

public class ThreadAttacher : MonoBehaviour
{
    [SerializeField]
    private LineRenderer lineRenderer;

    [SerializeField]
    private LayerMask attachableLayers;
    [SerializeField]
    private float maxDistance;

    private void Update()
    {
        for (int i = 0; i <= 1; i++)
        {
            if (Input.GetMouseButtonDown(i))
            {
                AttachThread(i);
            }
        }
    }

    private void AttachThread(int index)
    {
        if (Physics.Raycast(transform.position, transform.forward, out var hitInfo, maxDistance, attachableLayers))
        {
            var attachPosition = hitInfo.point;
            lineRenderer.SetPosition(index, attachPosition);
        }

    }
}
