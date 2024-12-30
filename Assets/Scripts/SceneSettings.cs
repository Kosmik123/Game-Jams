using UnityEngine;

namespace Bipolar.Pong
{
	public class SceneSettings : MonoBehaviour
	{
		public const float SceneBorder = 10;

		private static Camera mainCamera;


		private static float horizontalExtents;
		private static float HorizontalExtents
		{
			get
			{
#if UNITY_EDITOR
				Calculate();
#endif
				return horizontalExtents;
			}
		}

		public static float LeftBorder => -HorizontalExtents;
		public static float RightBorder => HorizontalExtents;

		private void Awake()
		{
			Calculate();
		}

		private static void Calculate()
		{
			mainCamera = Camera.main;
			horizontalExtents = mainCamera.orthographicSize * mainCamera.aspect;
		}
	}
}
