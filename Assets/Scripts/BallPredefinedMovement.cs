using UnityEngine;

namespace Bipolar.Pong
{
	[ExecuteAlways]
	public class BallPredefinedMovement : MonoBehaviour
	{
		[Header("References")]
		[SerializeField]
		private BallController ball;
		[SerializeField]
		private BatController leftPaddle;
		[SerializeField]
		private BatController rightPaddle;

		[Header("Settings")]
		[SerializeField]
		private float ballSpeed;
		[SerializeField]
		private Transform[] points;
		[SerializeField]
		private bool stopAtEnd = true;

		[Header("States")]
		[SerializeField]
		private int targetPointIndex = 0;
		[SerializeField]
		private bool isFinishing;
		[SerializeField]
		private bool paddlesReturn;

		private void Awake()
		{
			ValidatePointsPositions();
		}

		private void Start()
		{
			int last = points.Length - 1;
			ball.transform.position = Vector3.Lerp(points[0].position, points[last].position, 0.5f);
			targetPointIndex = 0;

		}

		private void Update()
		{
			if (Application.isPlaying)
			{
				UpdateGame();
			}
			else
			{
				ValidatePointsPositions();
			}
		}

		private void UpdateGame()
		{
			if (UnityEngine.Input.GetKeyDown(KeyCode.Space))
			{
				paddlesReturn = true;
			}
			if (paddlesReturn)
			{
				MovePaddleToStart(leftPaddle);
				MovePaddleToStart(rightPaddle);
			}

			var ballPosition = ball.transform.position;


			var target = points[targetPointIndex].position;
			float distanceToMove = ballSpeed * Time.deltaTime;
			float distanceToTarget = Vector2.Distance(ballPosition, target);
			//if (distanceToMove > distanceToTarget)
			//{
			//	targetPointIndex = (targetPointIndex + 1) % points.Length;
			//	target = points[targetPointIndex].position;
			//	distanceToMove -= distanceToTarget;
			//}

			ballPosition = Vector3.MoveTowards(ballPosition, target, distanceToMove);
			if (target == ballPosition)
				targetPointIndex = (targetPointIndex + 1) % points.Length;

			if (stopAtEnd)
			{
				int lastIndex = points.Length - 1;
				if (targetPointIndex == lastIndex)
				{
					isFinishing = true;
				}
				else if (isFinishing && targetPointIndex == 0)
				{
					float distanceToEnd = Vector2.Distance(ballPosition, points[0].position);
					float distanceFromStart = Vector2.Distance(points[lastIndex].position, ballPosition);
					if (distanceToEnd <= distanceFromStart)
					{
						UnityEditor.EditorApplication.isPlaying = false;
					}
				}
			}

			ball.transform.position = ballPosition;
		}

		private void MovePaddleToStart(BatController paddle)
		{
			paddle.enabled = false;
			float speed = paddle.MoveSpeed / 4f;
			var paddlePosition = paddle.transform.position;

			var target = new Vector2(paddlePosition.x, 0);
			paddle.transform.position = Vector2.MoveTowards(paddlePosition, target, speed * Time.deltaTime);
		}

		private void ValidatePointsPositions()
		{
			float radius = ball.Radius;
			var rect = new Rect
			{
				min = new Vector2(leftPaddle.transform.position.x + 0.25f + radius, -SceneSettings.SceneBorder + radius),
				max = new Vector2(rightPaddle.transform.position.x - 0.25f - radius, SceneSettings.SceneBorder - radius)
			};
			foreach (var point in points)
			{
				var position = point.position;
				position.x = Mathf.Clamp(position.x, rect.xMin, rect.xMax);
				position.y = Mathf.Clamp(position.y, rect.yMin, rect.yMax);
				point.position = position;
			}
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = Color.yellow;
			int count = points.Length;
			for (int i = 0; i < count; i++)
			{
				int nextPointIndex = (i + 1) % count;
				var start = points[i].position;
				var end = points[nextPointIndex].position;
				Gizmos.DrawLine(start, end);
			}

			Gizmos.color = Color.red;
			Gizmos.DrawSphere(points[targetPointIndex].position, 0.2f);
		}
	}
}