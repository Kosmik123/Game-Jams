using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bipolar.Match3
{
    public class GeneralBoardTokensMovementManager : TokensMovementManager
    {
        public override event System.Action OnTokensMovementStopped;

        private GeneralBoard _board;
        public GeneralBoard Board
        {
            get
            {
                if (_board == null)
                    _board = GetComponent<GeneralBoard>();
                return _board;
            }
        }

        private readonly Dictionary<Token, Coroutine> tokenMovementCoroutines = new Dictionary<Token, Coroutine>();
        public override bool AreTokensMoving => tokenMovementCoroutines.Count > 0;

        public void StartMovingTokenAlongLine(Token token, GeneralBoard.CoordsLine line, int fromIndex, int cellDistance)
        {
            var movementCoroutine = StartCoroutine(TokenMovementCo(token, line, fromIndex, cellDistance));
            tokenMovementCoroutines.Add(token, movementCoroutine);
        }

        private IEnumerator TokenMovementCo(Token token, GeneralBoard.CoordsLine line, int fromIndex, int cellDistance)
        {
            var startIndex = fromIndex;
            for (int i = 1; i <= cellDistance; i++)
            {
                int targetIndex = fromIndex + i;
                var targetCoord = line.Coords[targetIndex];

                var startPosition = startIndex < 0 ? token.transform.position : Board.CoordToWorld(line.Coords[startIndex]);
                var targetPosition = Board.CoordToWorld(targetCoord);
                float realDistance = Vector3.Distance(startPosition, targetPosition);

                float progressSpeed = 5f / realDistance;

                float progress = 0;
                while (progress < 1)
                {
                    token.transform.position = Vector3.Lerp(startPosition, targetPosition, progress);
                    yield return waitForFixedUpdate;
                    progress += Time.fixedDeltaTime * progressSpeed;
                }
                token.transform.position = targetPosition;
                startIndex = targetIndex;
            }

            tokenMovementCoroutines.Remove(token);
            if (tokenMovementCoroutines.Count <= 0)
            {
                Debug.Log("Koniec ruchu");
                OnTokensMovementStopped?.Invoke();
            }
        }
    }
}
