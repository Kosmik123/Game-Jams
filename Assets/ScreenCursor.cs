using UnityEngine;

public class ScreenCursor : MonoBehaviour
{
	private void Update() => transform.position = Input.mousePosition;
}
