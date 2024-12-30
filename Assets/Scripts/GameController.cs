using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Bipolar.Pong
{
	public delegate void PointScoredEventHandler(int playerIndex);

	public class GameController : MonoBehaviour
	{
		public event System.Action OnGameStarted;
		public event PointScoredEventHandler OnPointScored;
		public event PointScoredEventHandler OnPlayerWon;

		[Header("Settings")]
		[SerializeField]
		private BallController ball;
		[SerializeField, FormerlySerializedAs("bats")]
		private BatController[] paddles;
		[SerializeField]
		private float batsOffsetFromBorder;
		
		[Header("States")]
		private int nextStartingPlayer;
		[SerializeField]
		private int[] playerScores;
		public IReadOnlyList<int> PlayerScores => playerScores;

		private void Awake()
		{
			playerScores = new int[paddles.Length];
			//SetBatPosition(0, SceneSettings.LeftBorder + batsOffsetFromBorder);
			//SetBatPosition(1, SceneSettings.RightBorder - batsOffsetFromBorder);
		}

		private void SetBatPosition(int batIndex, float xPosition)
		{
			var bat = paddles[batIndex];
			var position = bat.transform.position;
			position.x = xPosition;
			bat.transform.position = position;
		}

		private void OnEnable()
		{
			ball.OnBorderExceeded += Ball_OnBorderExceeded;
		}

		private void Start()
		{
			RestartGame();
		}

		private void RestartGame()
		{
			for (int i = 0; i < 2; i++)
			{
				playerScores[i] = 0;
				OnPointScored?.Invoke(i);
			}

			nextStartingPlayer = Random.Range(0, 2);
			StartCoroutine(StartGameAfterDelay());
			OnGameStarted?.Invoke();
		}

		private void StartGame(int startingPlayerIndex)
		{
			var direction = new Vector2(
				IndexToAxisValue(startingPlayerIndex),
				Random.Range(-2f, 2f));

			ball.gameObject.SetActive(true);
			ball.StartMoving(direction);
		}

		public static float IndexToAxisValue(int index) => index == 0 ? -1 : 1;

		private void Ball_OnBorderExceeded(int side)
		{
			int scoringPlayer = side > 0 ? 0 : 1;
			playerScores[scoringPlayer]++;
			ball.gameObject.SetActive(false);
			ball.transform.position = Vector3.zero;
			nextStartingPlayer = scoringPlayer;
			OnPointScored?.Invoke(scoringPlayer);
			for (int i = 0; i < 2; i++)
			{
				if (playerScores[i] >= 10)
				{
					OnPlayerWon?.Invoke(scoringPlayer);
					StartCoroutine(RestartGameAfterAnyKey());
					return;
				}
			}

			StartCoroutine(StartGameAfterDelay());
		}

		private IEnumerator StartGameAfterDelay()
		{
			yield return new WaitForSeconds(2.5f);
			StartGame(nextStartingPlayer);
			nextStartingPlayer = -1;
		}

		private IEnumerator RestartGameAfterAnyKey()
		{
			yield return new WaitForSeconds(3);
			yield return new WaitUntil(() => UnityEngine.Input.anyKey);
			RestartGame();
		}

		private void OnDisable()
		{
			ball.OnBorderExceeded -= Ball_OnBorderExceeded;
		}
	}
}
