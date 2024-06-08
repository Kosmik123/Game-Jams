using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private Transform head;

    private void LateUpdate()
    {
        transform.SetPositionAndRotation(head.position, head.rotation);
    }
}
