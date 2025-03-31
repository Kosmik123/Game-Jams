using UnityEngine;

public class CubeRotation : MonoBehaviour
{
	[SerializeField]
	private Vector2 sensitivity = Vector2.one;

	private void Update()
	{
		if (Input.GetMouseButton(1) && !Input.GetMouseButton(0))
		{
			float x = -Input.GetAxis("Mouse X") * sensitivity.x;
			float y = Input.GetAxis("Mouse Y") * sensitivity.y;

			transform.Rotate(Vector3.Project(Vector3.up, transform.up), x, Space.World);
			transform.Rotate(Vector3.right, y, Space.World);
		}
	}
}
