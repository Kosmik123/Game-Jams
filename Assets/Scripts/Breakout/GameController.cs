using NaughtyAttributes;
using System.Collections;
using UnityEngine;

namespace Bipolar.Breakout
{
	public class GameController : MonoBehaviour
    {
        public event System.Action OnGameOver;
        public event System.Action OnGameStarted;

        private const string highscoreKey = "HighScore";

        [SerializeField]
        private BallController ballPrototype;
        [SerializeField]
        private Paddle paddle;
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
        [SerializeField, ReadOnly]
        private int score;
        [SerializeField, ReadOnly]
        private int highScore;

		private void Start()
		{
            highScore = PlayerPrefs.GetInt(highscoreKey, 0);
			RestartGame();
		}

		private void StartGame()
		{
            score = 0;
            currentLevel = Instantiate(level, levelHolder);

            paddle.Movement.enabled = true;
			currentBall = Instantiate(ballPrototype, paddle.BallOrigin);
			currentBall.OnBounced += IncreaseBallSpeed;
			BrickController.OnBrickBroke += BrickController_OnBrickBroke;

			StartCoroutine(LaunchBallOnPlayerInput());
            OnGameStarted?.Invoke();
		}

		private void BrickController_OnBrickBroke(int points)
		{
            score += points;
		}

		public void RestartGame()
        {
            DisposeGame();
            StartGame();
        }

		private void DisposeGame()
		{
			isPlaying = false;
			StopAllCoroutines();
			currentBall.DestroyObject();
			currentLevel.DestroyObject();
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
            if (isPlaying == false)
                return;

            if (currentBall.transform.position.y < losingHeight.position.y)
                GameOver();
		}

		private void GameOver()
		{
			paddle.Movement.enabled = false;
			currentBall.OnBounced -= IncreaseBallSpeed;
            currentBall.MoveSpeed = 0;

            if (score > highScore)
            {
                highScore = score;
                PlayerPrefs.SetInt(highscoreKey, highScore);
            }

            OnGameOver?.Invoke();
		}
	}
}

public static class DestroyExtensions
{
    public static void DestroyObject(this Object @object)
    {
        if (@object is GameObject gameObject)
			Object.Destroy(gameObject);
        else if (@object is Component component)
			Object.Destroy(component.gameObject);
    }
}
