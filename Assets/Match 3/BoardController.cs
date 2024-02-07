using System.Collections.Generic;
using UnityEngine;

namespace Bipolar.Match3
{
    [RequireComponent(typeof(Board))]
    public class BoardController : MonoBehaviour
    {
        public delegate void TokensSwapEventHandler(Vector2Int tokenCoord1, Vector2Int tokenCoord2);

        public event System.Action OnTokensMovementStopped;
        public event System.Action OnTokensColapsed;
        public event TokensSwapEventHandler OnTokensSwapped;

        private Board _board;
        public Board Board
        {
            get
            {
                if (_board == null)
                    _board = GetComponent<Board>();
                return _board;
            }
        }

        [SerializeField]
        private Settings settings;
        [SerializeField]
        private TokensSpawner tokensSpawner;

        [SerializeField]
        private Vector2Int CollapseDirection;
        
        private readonly List<Token> currentlyMovingTokens = new List<Token>();
        public bool AreTokensMoving => currentlyMovingTokens.Count > 0;

        private void Start()
        {
            PopulateBoard();
        }

        private void PopulateBoard()
        {
            var tokens = new Token[Board.Dimentions.y, Board.Dimentions.x];

            OnTokensMovementStopped += CallColapseEvent;
            var spawnOffset = CollapseDirection;
            spawnOffset.Scale(Board.Dimentions);

            for (int j = 0; j < Board.Dimentions.y; j++)
            {
                for (int i = 0; i < Board.Dimentions.x; i++)
                {
                    var spawnCoord = new Vector2Int(i, j) - spawnOffset;
                    var token = CreateToken(spawnCoord.x, spawnCoord.y, true);
                    StartTokenMovement(token, i, j);
                    tokens[j, i] = token;
                }
            }
            Board.SetTokens(tokens);
        }

        private void CallColapseEvent()
        {
            OnTokensMovementStopped -= CallColapseEvent;
            OnTokensColapsed?.Invoke();
        }

        private Token CreateToken(int xCoord, int yCoord, bool avoidMatches = false)
        {
            var spawnCoord = new Vector2Int(xCoord, yCoord);
            Vector3 spawnPosition = Board.CoordToWorld(spawnCoord);
            var token = tokensSpawner.SpawnToken();
            token.transform.position = spawnPosition;
            token.gameObject.name = $"Token {xCoord}:{yCoord}";
            token.Type = settings.TokenTypes[Random.Range(0, settings.TokenTypes.Count)];
            return token;
        }

        public void Collapse()
        {
            int iterationAxis = (CollapseDirection.x != 0) ? 1 : 0;
            bool colapsed = false;
            for (int lineIndex = 0; lineIndex < Board.Dimentions[iterationAxis]; lineIndex++)
            {
                int emptyCellsCount = CollapseTokensInLine(lineIndex, iterationAxis);
                if (emptyCellsCount > 0)
                {
                    colapsed = true;
                    RefillLine(lineIndex, emptyCellsCount, iterationAxis);
                }
            }

            if (colapsed)
                OnTokensMovementStopped += CallColapseEvent;
        }

        private int CollapseTokensInLine(int lineIndex, int iterationAxis)
        {
            int collapseAxis = 1 - iterationAxis; // to samo
            int lineSize = Board.Dimentions[collapseAxis]; // to samo

            int startCellIndex = CollapseDirection[collapseAxis] > 0 ? -1 : 0; // odwrócony warunek

            int nonExistingTokensCount = 0;
            for (int i = 0; i < lineSize; i++) // troszkę inne
            {
                var coord = Vector2Int.zero; // to samo
                coord[iterationAxis] = lineIndex; // to samo
                coord[collapseAxis] = (startCellIndex + i * -CollapseDirection[collapseAxis] + lineSize) % lineSize; // odwrócony znak

                // odtąd są inne czynnności
                var token = Board.GetToken(coord);
                if (token == null || token.IsDestroyed)
                {
                    nonExistingTokensCount++;
                }
                else if (nonExistingTokensCount > 0)
                {
                    var offsetToMove = CollapseDirection * nonExistingTokensCount;
                    var targetCoord = coord + offsetToMove;
                    Board.SetToken(null, coord);
                    Board.SetToken(token, targetCoord);
                    StartTokenMovement(token, targetCoord);
                }
            }

            return nonExistingTokensCount;
        }

        private void RefillLine(int lineIndex, int count, int iterationAxis)
        {
            int collapseAxis = 1 - iterationAxis;
            int lineSize = Board.Dimentions[collapseAxis];

            int startCellIndex = CollapseDirection[collapseAxis] < 0 ? -1 : 0;

            var spawnOffset = -CollapseDirection * count;
            for (int i = 0; i < count; i++) 
            {
                var coord = Vector2Int.zero;
                coord[iterationAxis] = lineIndex;
                coord[collapseAxis] = (startCellIndex + i * CollapseDirection[collapseAxis] + lineSize) % lineSize;

                // odtąd inne czynnności
                var spawnCoord = coord + spawnOffset;
                var newToken = CreateToken(spawnCoord.x, spawnCoord.y, false);
                Board.SetToken(newToken, coord);
                StartTokenMovement(newToken, coord);
            }
        }

        private System.Action swapEndedCallback;
        public void SwapTokens(Vector2Int token1Coord, Vector2Int token2Coord)
        {
            var token1 = Board.GetToken(token1Coord);
            var token2 = Board.GetToken(token2Coord);

            StartTokenMovement(token2, token1Coord);
            StartTokenMovement(token1, token2Coord);

            Board.SwapTokens(token1Coord, token2Coord);
            OnTokensMovementStopped += BoardController_OnTokensMovementStopped;
            swapEndedCallback = () =>
            {
                swapEndedCallback = null;
                OnTokensSwapped?.Invoke(token1Coord, token2Coord);
            };
        }

        private void BoardController_OnTokensMovementStopped()
        {
            OnTokensMovementStopped -= BoardController_OnTokensMovementStopped;
            swapEndedCallback.Invoke();
        }

        public void StartTokenMovement(Token token, Vector2Int targetCoord) => StartTokenMovement(token, targetCoord.x, targetCoord.y);
        public void StartTokenMovement(Token token, int xTargetCoord, int yTargetCoord)
        {
            token.OnMovementEnded += Token_OnMovementEnded;
            token.BeginMovingToPosition(Board.CoordToWorld(xTargetCoord, yTargetCoord));
            currentlyMovingTokens.Add(token);
        }

        private void Token_OnMovementEnded(Token token)
        {
            token.OnMovementEnded -= Token_OnMovementEnded;
            currentlyMovingTokens.Remove(token);
            if (AreTokensMoving == false)
            {
                OnTokensMovementStopped?.Invoke();
            }
        }
    }
}
