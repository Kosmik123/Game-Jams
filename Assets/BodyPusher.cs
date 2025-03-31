using UnityEngine;

public class BodyPusher : MonoBehaviour
{
	[SerializeField]
	private Rigidbody body;

	[SerializeField]
	private Transform forcePoint;

	[SerializeField]
	private float forceScale;

	[SerializeField]
	private Transform referencePoint;
	[SerializeField]
	private AlignToSurface tongueTip;

	private bool isPulling;
	private void FixedUpdate()
	{

	}

	private void Update()
	{
		isPulling = Input.GetMouseButton(0);
		tongueTip.enabled = !isPulling; 
		
		if (isPulling)
		{
			var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out var hitInfo, float.PositiveInfinity, Physics.AllLayers))
			{
				Vector3 difference = hitInfo.point - referencePoint.position;
				body.transform.Translate(-difference);
				//body.AddForceAtPosition(forceScale * difference, forcePoint.position, ForceMode.Acceleration);
			}
		}
	}
}
