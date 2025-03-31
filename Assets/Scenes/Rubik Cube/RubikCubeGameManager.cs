using UnityEngine;

public class RubikCubeGameManager : MonoBehaviour
{
	[SerializeField]
	private CubeRotator cube;

	private void Start()
	{
		StartGame();
	}

	private void StartGame()
	{
		cube.OnSnapped += CubeRotator_OnSnapped;
	}

	private void CubeRotator_OnSnapped()
	{
		if (cube.RubikCube.IsSolved())
			Debug.Log("Solved");
	}
}
