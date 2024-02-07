using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Bipolar.Match3
{
    public delegate void TokensSwapEventHandler(Vector2Int tokenCoord1, Vector2Int tokenCoord2);

    [RequireComponent(typeof(Board))]
    public class BoardController : MonoBehaviour
    {
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
        private MoveDirection collapseDirection;
        public Vector2Int CollapseDirection
        { 
            get
            {
                return collapseDirection switch
                {
                    MoveDirection.Up => Vector2Int.up,
                    MoveDirection.Left => Vector2Int.left,
                    MoveDirection.Right => Vector2Int.right,
                    _ => Vector2Int.down,
                };
            } 
        }
        
        private readonly List<TokenMovement> currentlyMovingTokens = new List<TokenMovement>();
        public bool AreTokensMoving => currentlyMovingTokens.Count > 0;

        private void Start()
        {
            Collapse();
        }

        //private void PopulateBoard()
        //{
        //    var tokens = new Token[Board.Dimentions.y, Board.Dimentions.x];

        //    OnTokensMovementStopped += CallColapseEvent;
        //    var spawnOffset = CollapseDirection;
        //    spawnOffset.Scale(Board.Dimentions);

        //    for (int j = 0; j < Board.Dimentions.y; j++)
        //    {
        //        for (int i = 0; i < Board.Dimentions.x; i++)
        //        {
        //            var spawnCoord = new Vector2Int(i, j) - spawnOffset;
        //            var token = CreateToken(spawnCoord.x, spawnCoord.y, true);
        //            StartTokenMovement(token, i, j);
        //            tokens[j, i] = token;
        //        }
        //    }
        //    Board.SetTokens(tokens);
        //}

        private void CallCollapseEvent()
        {
            OnTokensMovementStopped -= CallCollapseEvent;
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
                OnTokensMovementStopped += CallCollapseEvent;
        }

        private int CollapseTokensInLine(int lineIndex, int iterationAxis)
        {
            int collapseAxis = 1 - iterationAxis; // to samo
            int lineSize = Board.Dimentions[collapseAxis]; // to samo

            int startCellIndex = CollapseDirection[collapseAxis] > 0 ? -1 : 0; // odwrócony warunek

            int nonExistingTokensCount = 0; // inna rzecz
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
                    StartTokenMovement(token, targetCoord, 0.5f); // to samo
                }
            }

            return nonExistingTokensCount;
        }

        private void RefillLine(int lineIndex, int count, int iterationAxis)
        {
            int collapseAxis = 1 - iterationAxis; // to samo
            int lineSize = Board.Dimentions[collapseAxis]; // to samo

            int startCellIndex = CollapseDirection[collapseAxis] < 0 ? -1 : 0; // odwrócony warunek

            var spawnOffset = -CollapseDirection * count; // inna rzecz
            for (int i = 0; i < count; i++) 
            {
                var coord = Vector2Int.zero; // to samo
                coord[iterationAxis] = lineIndex; // to samo
                coord[collapseAxis] = (startCellIndex + i * CollapseDirection[collapseAxis] + lineSize) % lineSize; // odwrócony znak

                // odtąd inne czynnności
                var spawnCoord = coord + spawnOffset;
                var newToken = CreateToken(spawnCoord.x, spawnCoord.y, false);
                Board.SetToken(newToken, coord);
                StartTokenMovement(newToken, coord, 0.5f); // to samo
            }
        }

        private System.Action swapEndedCallback;
        public void SwapTokens(Vector2Int tokenCoord1, Vector2Int tokenCoord2)
        {
            var token1 = Board.GetToken(tokenCoord1);
            var token2 = Board.GetToken(tokenCoord2);

            StartTokenMovement(token2, tokenCoord1);
            StartTokenMovement(token1, tokenCoord2);

            Board.SwapTokens(tokenCoord1, tokenCoord2);
            OnTokensMovementStopped += BoardController_OnTokensMovementStopped;
            swapEndedCallback = () =>
            {
                swapEndedCallback = null;
                OnTokensSwapped?.Invoke(tokenCoord1, tokenCoord2);
            };
        }

        private void BoardController_OnTokensMovementStopped()
        {
            OnTokensMovementStopped -= BoardController_OnTokensMovementStopped;
            swapEndedCallback.Invoke();
        }

        public void StartTokenMovement(Token token, Vector2Int targetCoord, float duration = -1) => StartTokenMovement(token, targetCoord.x, targetCoord.y, duration);
        public void StartTokenMovement(Token token, int xTargetCoord, int yTargetCoord, float duration = -1)
        {
            if (token.TryGetComponent<TokenMovement>(out var tokenMovement))
            {
                tokenMovement.OnMovementEnded += Token_OnMovementEnded;
                tokenMovement.BeginMovingToPosition(Board.CoordToWorld(xTargetCoord, yTargetCoord), duration);
                currentlyMovingTokens.Add(tokenMovement);
            } 
        }

        private void Token_OnMovementEnded(TokenMovement tokenMovement)
        {
            tokenMovement.OnMovementEnded -= Token_OnMovementEnded;
            currentlyMovingTokens.Remove(tokenMovement);
            CheckMovementFinish();
        }

        private void CheckMovementFinish()
        {
            if (AreTokensMoving == false)
                OnTokensMovementStopped?.Invoke();
        }
    }
}
