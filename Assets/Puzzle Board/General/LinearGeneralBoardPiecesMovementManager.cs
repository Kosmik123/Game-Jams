using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bipolar.PuzzleBoard.General
{
    [RequireComponent(typeof(GeneralBoard))]
    public class LinearGeneralBoardPiecesMovementManager : PiecesMovementManager
    {
        public override event System.Action OnAllPiecesMovementStopped;

        [SerializeField]
        private float piecesMovementSpeed = 8f;

        private GeneralBoard _board;
        public GeneralBoard Board
        {
            get
            {
                if (_board == null && this)
                    _board = GetComponent<GeneralBoard>();
                return _board;
            }
        }

        private readonly Dictionary<Piece, Coroutine> pieceMovementCoroutines = new Dictionary<Piece, Coroutine>();
        public override bool ArePiecesMoving => pieceMovementCoroutines.Count > 0;

        public void StartPieceMovement(Piece piece, CoordsLine line, int fromIndex, int cellDistance)
        {
            var movementCoroutine = StartCoroutine(MovementCo(piece, line, fromIndex, cellDistance));
            pieceMovementCoroutines.Add(piece, movementCoroutine);
        }

        private IEnumerator MovementCo(Piece piece, CoordsLine line, int fromIndex, int cellDistance)
        {
            var startIndex = fromIndex;
            for (int i = 1; i <= cellDistance; i++)
            {
                int targetIndex = fromIndex + i;
                var targetCoord = line.Coords[targetIndex];

                var startPosition = startIndex < 0 ? piece.transform.position : Board.CoordToWorld(line.Coords[startIndex]);
                var targetPosition = Board.CoordToWorld(targetCoord);
                float realDistance = Vector3.Distance(startPosition, targetPosition);

                float progressSpeed = piecesMovementSpeed / realDistance;

                float progress = 0;
                while (progress < 1)
                {
                    piece.transform.position = Vector3.Lerp(startPosition, targetPosition, progress);
                    yield return waitForFixedUpdate;
                    progress += Time.fixedDeltaTime * progressSpeed;
                }
                piece.transform.position = targetPosition;
                startIndex = targetIndex;
            }

            pieceMovementCoroutines.Remove(piece);
            if (ArePiecesMoving == false)
                OnAllPiecesMovementStopped?.Invoke();
        }
    }
}
