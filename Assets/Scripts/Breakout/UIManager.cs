using UnityEngine;

namespace Bipolar.Breakout
{
	public class UIManager : MonoBehaviour
    {
        private GameController gameController;

		private void Awake()
		{
			gameController = FindAnyObjectByType<GameController>();
		}

		private void OnEnable()
		{
            gameController.OnGameOver += OnGameOver;
		}

		private void OnGameOver()
		{

		}

		private void OnDisable()
		{
            gameController.OnGameOver -= OnGameOver;
		}
	}
}
