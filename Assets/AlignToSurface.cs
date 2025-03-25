using UnityEngine;

public class AlignToSurface : MonoBehaviour
{
	[SerializeField]
	private LayerMask detectedLayers = Physics.AllLayers;
	[SerializeField]
	private float offset;

	private Camera viewCamera;

	private RaycastHit hitInfo;
	private Vector3 mousePosition;

	private void Awake()
	{
		viewCamera = Camera.main;
	}

	private void Start()
	{
		mousePosition = Input.mousePosition;
	}

	private void FixedUpdate()
	{
		var ray = viewCamera.ScreenPointToRay(mousePosition);
		Physics.Raycast(ray, out hitInfo, float.PositiveInfinity, detectedLayers);
	}

	private void Update()
	{
		mousePosition = Input.mousePosition;
		if (hitInfo.collider)
		{
			transform.position = hitInfo.point + offset * hitInfo.normal;
			transform.up = hitInfo.normal;
		}
	}
}
