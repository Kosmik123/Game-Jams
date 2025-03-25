using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(SplineExtrude))]
public class SplineExtrudeConstantSegmentsCount : MonoBehaviour
{
	private SplineExtrude splineExtrude;

	[SerializeField, Min(1)]
	private float segmentsCount = 20;

	private void Awake()
	{
		splineExtrude = GetComponent<SplineExtrude>();
	}

	private void Update()
	{
		var splineLength = splineExtrude.Spline.GetLength();
		splineExtrude.SegmentsPerUnit = segmentsCount / splineLength;
	}
}
