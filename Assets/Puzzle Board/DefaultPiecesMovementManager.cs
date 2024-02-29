using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bipolar.PuzzleBoard
{
    public delegate void PieceMovementEndEventHandler(Piece piece);

    [RequireComponent(typeof(Board))]
    public class DefaultPiecesMovementManager : PiecesMovementManager
    {
        public override event System.Action OnAllPiecesMovementStopped;
        public event PieceMovementEndEventHandler OnPieceMovementEnded;

        [SerializeField]
        private Board board;
        [SerializeField]
        private float defaultMovementDuration = 0.2f;

        private readonly Dictionary<Piece, Coroutine> pieceMovementCoroutines = new Dictionary<Piece, Coroutine>();
        public override bool ArePiecesMoving => pieceMovementCoroutines.Count > 0;

        public void StartPieceMovement(Piece piece, Vector2Int targetCoord, float duration = -1) 
        {
            if (duration < 0)
                duration = defaultMovementDuration;
            var movementCoroutine = StartCoroutine(MovementCo(piece, board.CoordToWorld(targetCoord), duration));
            pieceMovementCoroutines.Add(piece, movementCoroutine);
        }

        private IEnumerator MovementCo(Piece piece, Vector3 target, float duration)
        {
            Vector3 startPosition = piece.transform.position;
            Vector3 targetPosition = target;
            float moveProgress = 0;
            float progressSpeed = 1f / duration;
            while (moveProgress < 1)
            {
                moveProgress += progressSpeed * Time.deltaTime;
                piece.transform.position = Vector3.Lerp(startPosition, targetPosition, moveProgress);
                yield return null;
            }
            piece.transform.position = targetPosition;

            pieceMovementCoroutines.Remove(piece);
            OnPieceMovementEnded?.Invoke(piece);
            if (ArePiecesMoving == false)
                OnAllPiecesMovementStopped?.Invoke();
        }
    }
}
