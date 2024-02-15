using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bipolar.Match3
{
    public class GeneralBoardController : MonoBehaviour
    {
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

        [SerializeField]
        private TokensSpawner tokensSpawner;

        [SerializeField]
        private TokenTypeProvider tokenTypeProvider;
        public TokenTypeProvider TokenTypeProvider
        {
            get => tokenTypeProvider;
            set => tokenTypeProvider = value;
        }

        private readonly List<TokenMovement> currentlyMovingTokens = new List<TokenMovement>();
        public bool AreTokensMoving => currentlyMovingTokens.Count > 0;

        private readonly Dictionary<Token, Coroutine> tokenMovementCoroutines = new Dictionary<Token, Coroutine>();


        private void Start()
        {
            Collapse();
        }

        public void Collapse()
        {
            foreach (var line in Board.Lines)
            {
                int emptyCellsCount = CollapseTokensInLine(line);
                bool colapsed = false;
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
            for (int i = 1; i < cellDistance; i++)
            {
                int targetIndex = fromIndex + i;
                var startCoord = line.Coords[startIndex];
                var targetCoord = line.Coords[targetIndex];

                var startPosition = Board.CoordToWorld(startCoord);
                var targetPosition = Board.CoordToWorld(targetCoord);

                float progress = 0;
                while (progress < 1)
                {
                    progress += Time.deltaTime;
                    token.transform.position = Vector3.Lerp(startPosition, targetPosition, progress); 
                    yield return null;
                }
                token.transform.position = targetPosition;
                startIndex = targetIndex;
            }

            tokenMovementCoroutines.Remove(token);
        }

        private void RefillLine(GeneralBoard.CoordsLine line, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var coord = line.Coords[i];
                var newToken = CreateToken(coord.x, coord.y);
                //newToken.transform.position = 
            }
        }

        private Token CreateToken(int x, int y)
        {
            var token = tokensSpawner.SpawnToken();
            token.Type = tokenTypeProvider.GetTokenType(x, y);
            return token;
        }

        private void OnDisable()
        {
            
        }
    }
}
