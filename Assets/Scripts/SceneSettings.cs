using UnityEngine;

namespace Bipolar.Pong
{
	public class SceneSettings : MonoBehaviour
	{
		public const float SceneBorder = 10;

		private Camera mainCamera;

		private static float horizontalExtents;

		public static float LeftBorder => -horizontalExtents;
		public static float RightBorder => horizontalExtents;

		private void Awake()
		{
			mainCamera = Camera.main;
			horizontalExtents = mainCamera.orthographicSize * mainCamera.aspect;
		}
	}
}
