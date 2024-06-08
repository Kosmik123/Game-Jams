using UnityEngine;

public class ThreadAttacher : MonoBehaviour
{
    [SerializeField]
    private ThreadPhysics thread;

    [SerializeField]
    private LayerMask attachableLayers;
    [SerializeField]
    private float maxDistance;

    private void Update()
    {
        for (int i = 0; i < 1; i++)
        {
            if (Input.GetMouseButtonDown(i))
            {
                AttachThread(i);
            }
        }

        if (Input.GetMouseButtonDown(1))
            thread.gameObject.SetActive(false);
    }

    private void AttachThread(int index)
    {
        if (Physics.Raycast(transform.position, transform.forward, out var hitInfo, maxDistance, attachableLayers))
        {
            var attachPosition = hitInfo.point + thread.Thickness * hitInfo.normal;
            thread.transform.position = attachPosition;
            thread.gameObject.SetActive(true);
        }   
    }
}
