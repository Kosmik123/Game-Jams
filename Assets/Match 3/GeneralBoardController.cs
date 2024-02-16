using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bipolar.Match3
{
    [RequireComponent(typeof(GeneralBoard))]
    public class GeneralBoardController : BoardController<GeneralBoard>
    {
        public override event System.Action OnTokensMovementStopped;
        
        private readonly List<TokenMovement> currentlyMovingTokens = new List<TokenMovement>();
        public bool AreTokensMoving => currentlyMovingTokens.Count > 0;

        private readonly Dictionary<Token, Coroutine> tokenMovementCoroutines = new Dictionary<Token, Coroutine>();

        private void Start()
        {
            Collapse();
        }

        public override void Collapse()
        {
            bool colapsed = false;
            foreach (var line in board.Lines)
            {
                int emptyCellsCount = CollapseTokensInLine(line);
                if (emptyCellsCount > 0)
                {
                    colapsed = true;
                    RefillLine(line, emptyCellsCount);
                }
            }
        }

        private int CollapseTokensInLine(GeneralBoard.CoordsLine line)
        {
            int nonExistingTokensCount = 0;
            for (int index = 0; index < line.Coords.Count; index++)
            {
                var coord = line.Coords[index];
                var token = Board.GetToken(coord);
                if (token == null || token.IsCleared)
                {
                    nonExistingTokensCount++;
                }
                else if (nonExistingTokensCount > 0)
                {
                    StartTokenMovingTokenAlongLine(token, line, index, nonExistingTokensCount);
                }
            }
            return nonExistingTokensCount;
        }

        private void StartTokenMovingTokenAlongLine(Token token, GeneralBoard.CoordsLine line, int fromIndex, int cellDistance)
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
                    yield return null;
                    progress += Time.deltaTime * progressSpeed;
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

        private void RefillLine(GeneralBoard.CoordsLine line, int count)
        {
            var startCoord = line.Coords[0];
            var creatingDirection = -board.GetDirection(startCoord);
            for (int i = 0; i < count; i++)
            {
                var coord = line.Coords[i];
                var newToken = CreateToken(coord.x, coord.y);
                var spawningCoord = startCoord + creatingDirection * (count - i);
                var spawningPosition = board.CoordToWorld(spawningCoord);
                newToken.transform.position = spawningPosition;
                StartTokenMovingTokenAlongLine(newToken, line, -1, i + 1);
            }
        }
        
        private void OnDisable()
        {
            
        }
    }
}
