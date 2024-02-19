using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ComponentExtensions
{
    public static T GetCachedComponent<T>(this Component owner, ref T component) where T : Component
    {
        if (component == null)
            component = owner.GetComponent<T>();
        return component;
    }
}

namespace Bipolar.Match3
{
    [RequireComponent(typeof(RectangularBoard))]
    public class RectangularBoardTokensMovementManager : PiecesMovementManager
    {
        public override event System.Action OnPiecesMovementStopped;

        [SerializeField]
        private RectangularBoard board;
        [SerializeField]
        private float defaultMovementDuration;

        private readonly Dictionary<Piece, Coroutine> tokenMovementCoroutines = new Dictionary<Piece, Coroutine>();
        public override bool ArePiecesMoving => tokenMovementCoroutines.Count > 0;

        public void StartTokenMovement(Piece token, Vector2Int targetCoord, float duration = -1) 
        {
            if (duration < 0)
                duration = defaultMovementDuration;
            var movementCoroutine = StartCoroutine(MovementCo(token, board.CoordToWorld(targetCoord), duration));
            tokenMovementCoroutines.Add(token, movementCoroutine);
        }

        private IEnumerator MovementCo(Piece token, Vector3 target, float duration)
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
            if (ArePiecesMoving == false)
                OnPiecesMovementStopped?.Invoke();
        }
    }
}
