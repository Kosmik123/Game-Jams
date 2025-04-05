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
		private BoxCollider2D gameBounds;
        [SerializeField]
        private float beforeBallSpawnDuration;

		[Header("Ball")]
        [SerializeField]
        private float ballSpeedMultiplier = 1;
        [SerializeField]
        private float ballSpeedIncrease;

        [Header("Sounds")]
        [SerializeField]
        private AudioSource soundsSource;
        [SerializeField]
        private AudioClip gameStartSound;
        [SerializeField]
        private AudioClip ballLaunchSound;
        [SerializeField]
        private AudioClip gameOverSound;
        [SerializeField]
        private AudioClip victorySound;

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

		private IEnumerator StartGameCo()
		{
            score = 0;
            currentLevel = Instantiate(level, levelHolder);
            soundsSource.PlayOneShot(gameStartSound);
            OnGameStarted?.Invoke();

            yield return new WaitForSeconds(beforeBallSpawnDuration);

			currentBall = Instantiate(ballPrototype, paddle.BallOrigin);
			currentBall.OnBounced += IncreaseBallSpeed;
            paddle.Movement.enabled = true;
			BrickController.OnBrickBroke += BrickController_OnBrickBroke;

			StartCoroutine(LaunchBallOnPlayerInput());
		}

		private void BrickController_OnBrickBroke(int points)
		{
            score += points;
		}

		public void RestartGame()
        {
            DisposeGame();
            StartCoroutine(StartGameCo());
        }

		private void DisposeGame()
		{
			isPlaying = false;
			BrickController.OnBrickBroke -= BrickController_OnBrickBroke;
			StopAllCoroutines();
			currentBall.DestroyObject();
			currentLevel.DestroyObject();
		}

		private void IncreaseBallSpeed(Collision2D c)
		{
            currentBall.MoveSpeed = currentBall.MoveSpeed * ballSpeedMultiplier + ballSpeedIncrease;
		}

		public IEnumerator LaunchBallOnPlayerInput()
        {
            yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
            soundsSource.PlayOneShot(ballLaunchSound);
            var launchDirection = new Vector2(Random.Range(-1, 1), 1);
            currentBall.transform.parent = null;
            currentBall.SetMovement(launchDirection);
            isPlaying = true;
        }

		private void Update()
		{
            if (isPlaying == false)
                return;

            if (gameBounds.bounds.Contains(currentBall.transform.position) == false)
                GameOver();
		}

		private void GameOver()
		{
            isPlaying = false;
			paddle.Movement.enabled = false;
			currentBall.OnBounced -= IncreaseBallSpeed;
            currentBall.MoveSpeed = 0;

            soundsSource.PlayOneShot(gameOverSound);
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
