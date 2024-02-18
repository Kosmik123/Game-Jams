using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bipolar.Match3
{
    [RequireComponent(typeof(RectangularBoard))]
    public class RectangularBoardTokensMovementManager : TokensMovementManager
    {
        public override event System.Action OnTokensMovementStopped;

        [SerializeField]
        private RectangularBoard board;
        [SerializeField]
        private float defaultMovementDuration;

        private readonly Dictionary<Token, Coroutine> tokenMovementCoroutines = new Dictionary<Token, Coroutine>();
        public override bool AreTokensMoving => tokenMovementCoroutines.Count > 0;

        public void StartTokenMovement(Token token, Vector2Int targetCoord, float duration = -1) 
        {
            if (duration < 0)
                duration = defaultMovementDuration;
            var movementCoroutine = StartCoroutine(MovementCo(token, board.CoordToWorld(targetCoord), duration));
            tokenMovementCoroutines.Add(token, movementCoroutine);
        }

        private IEnumerator MovementCo(Token token, Vector3 target, float duration)
        {
            Vector3 startPosition = token.transform.position;
            Vector3 targetPosition = target;
            float moveProgress = 0;
            float progressSpeed = 1f / duration;
            while (moveProgress < 1)
            {
                moveProgress += progressSpeed * Time.deltaTime;
                token.transform.position = Vector3.Lerp(startPosition, targetPosition, moveProgress);
                yield return null;
            }
            token.transform.position = targetPosition;

            tokenMovementCoroutines.Remove(token);
            if (AreTokensMoving == false)
                OnTokensMovementStopped?.Invoke();
        }
    }
}
