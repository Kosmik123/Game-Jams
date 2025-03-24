using NaughtyAttributes;
using System.Collections;
using UnityEngine;

namespace Bipolar.Breakout
{

	public class GameController : MonoBehaviour
    {
        public event System.Action OnGameOver;

        [SerializeField]
        private BallController ballPrototype;
        [SerializeField]
        private PaddleController paddle;
        [SerializeField]
        private GameObject level;
        [SerializeField]
        private Transform levelHolder;
        [SerializeField]
        private Transform losingHeight;

        [SerializeField]
        private float ballSpeedMultiplier = 1;
        [SerializeField]
        private float ballSpeedIncrease;

        [Header("States")]
        [SerializeField, ReadOnly]
        private bool isPlaying;
        [SerializeField, ReadOnly]
        private GameObject currentLevel;
        [SerializeField, ReadOnly]
        private BallController currentBall;

		private void Start() => RestartGame();

		private void StartGame()
		{
            currentLevel = Instantiate(level, levelHolder);

			currentBall = Instantiate(ballPrototype, paddle.BallOrigin);
			currentBall.OnBounced += IncreaseBallSpeed;
			StartCoroutine(LaunchBallOnPlayerInput());
		}

        private void RestartGame()
        {
            DisposeGame();
            StartGame();
        }

		private void DisposeGame()
		{
			isPlaying = false;
			StopAllCoroutines();
			currentBall.Destroy();
			currentLevel.Destroy();
		}

		private void IncreaseBallSpeed()
		{
            currentBall.MoveSpeed = currentBall.MoveSpeed * ballSpeedMultiplier + ballSpeedIncrease;
		}

		public IEnumerator LaunchBallOnPlayerInput()
        {
            yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
            var launchDirection = new Vector2(Random.Range(-1, 1), 1);
            currentBall.transform.parent = null;
            currentBall.SetMovement(launchDirection);
            isPlaying = true;
        }

		private void Update()
		{
            if (currentBall.transform.position.y < losingHeight.position.y)
                GameOver();
		}

		private void GameOver()
		{
            currentBall.OnBounced -= IncreaseBallSpeed;
            currentBall.MoveSpeed = 0;
            OnGameOver?.Invoke();
		}
	}
}

public static class DestroyExtensions
{
    public static void Destroy(this Object @object)
    {
        if (@object is GameObject gameObject)
            Destroy(gameObject);
        else if (@object is Component component)
			Destroy(component.gameObject);
    }
}
