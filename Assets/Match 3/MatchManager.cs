using NaughtyAttributes;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Bipolar.Match3
{
    public class MatchManager : MonoBehaviour
    {
        public event System.Action<int> OnScoreChanged;

        [SerializeField]
        private BoardController boardController;
        [SerializeField]
        private SwapManager swapManager;

        [SerializeField, ReadOnly]
        private int score = 0;

        private void OnEnable()
        {
            boardController.OnTokensColapsed += BoardController_OnTokensColapsed;
            swapManager.OnSwapRequested += SwapManager_OnSwapRequested;
        }

        private void Start()
        {
            boardController.Collapse();
        }

        private void SwapManager_OnSwapRequested(Vector2Int tokenCoord1, Vector2Int tokenCoord2)
        {
            if (boardController.AreTokensMoving == false && currentlyClearedTokens.Count <= 0)
                SwapTokens(tokenCoord1, tokenCoord2);
        }

        private void SwapTokens(Vector2Int tokenCoord1, Vector2Int tokenCoord2)
        {
            boardController.SwapTokens(tokenCoord1, tokenCoord2);
            boardController.OnTokensSwapped += BoardController_OnTokensSwapped;
        }

        private void BoardController_OnTokensSwapped(Vector2Int tokenCoord1, Vector2Int tokenCoord2)
        {
            boardController.OnTokensSwapped -= BoardController_OnTokensSwapped;
            bool wasCorrectMove = FindMatches();
            if (wasCorrectMove == false)
            {
                boardController.SwapTokens(tokenCoord1, tokenCoord2);
            }
        }

        private List<TokensChain> tokenChains = new List<TokensChain>();
        private void BoardController_OnTokensColapsed()
        {
            FindMatches();
        }


        private readonly Queue<Vector2Int> coordsToCheck = new Queue<Vector2Int>();
        private bool FindMatches()
        {
            tokenChains.Clear();
            for (int j = 0; j < boardController.Board.Dimentions.y; j++)
            {
                for (int i = 0; i < boardController.Board.Dimentions.x; i++)
                {
                    Vector2Int coord = new Vector2Int(i, j);
                    if (tokenChains.FirstOrDefault(chain => chain.Contains(coord)) != null)
                        continue;

                    coordsToCheck.Clear();
                    coordsToCheck.Enqueue(coord);
                    var chain = new TokensChain()
                    {
                        TokenType = boardController.Board.GetToken(coord).Type
                    };

                    FindMatches(chain, coordsToCheck);

                    if (chain.IsMatchFound)
                        tokenChains.Add(chain);
                }
            }

            foreach (var chain in tokenChains)
                ClearChainTokens(chain);

            return tokenChains.Count > 0;
        }

        private static readonly Vector2Int[] chainsDirections =
        {
            Vector2Int.right,
            Vector2Int.up,
            Vector2Int.left,
            Vector2Int.down
        };

        private void FindMatches(TokensChain chain, Queue<Vector2Int> coordsToCheck)
        {
            while (coordsToCheck.Count > 0)
            {
                var tokenCoord = coordsToCheck.Dequeue();
                chain.Add(tokenCoord);
                foreach (var direction in chainsDirections)
                {
                    TryAddLineToChain(chain, tokenCoord, direction, coordsToCheck);
                }
            }
        }

        private bool TryAddLineToChain(TokensChain chain, Vector2Int tokenCoord, Vector2Int direction, Queue<Vector2Int> coordsToCheck)
        {
            var nearCoord = tokenCoord + direction;
            var nearToken = boardController.Board.GetToken(nearCoord);
            if (nearToken == null || chain.TokenType != nearToken.Type)
                return false;

            var backCoord = tokenCoord - direction;
            var backToken = boardController.Board.GetToken(backCoord);
            if (backToken && chain.TokenType == backToken.Type)
            {
                chain.IsMatchFound = true;
                TryEnqueueCoord(chain, coordsToCheck, nearCoord);
                TryEnqueueCoord(chain, coordsToCheck, backCoord);
                AddLineToChain(chain, tokenCoord, direction);
                return true;
            }

            var furtherCoord = nearCoord + direction;
            var furtherToken = boardController.Board.GetToken(furtherCoord);
            if (furtherToken && chain.TokenType == furtherToken.Type)
            {
                chain.IsMatchFound = true;
                TryEnqueueCoord(chain, coordsToCheck, nearCoord);
                TryEnqueueCoord(chain, coordsToCheck, furtherCoord);
                AddLineToChain(chain, nearCoord, direction);
                return true;
            }

            return false;
        }

        private bool TryEnqueueCoord(TokensChain chain, Queue<Vector2Int> coordToCheck, Vector2Int coord)
        {
            if (chain.Contains(coord))
                return false;

            if (coordsToCheck.Contains(coord))
                return false;

            coordsToCheck.Enqueue(coord);
            return true;
        }

        private void AddLineToChain(TokensChain chain, Vector2Int centerCoord, Vector2Int direction)
        {
            if (direction.x != 0)
                chain.AddHorizontal(centerCoord);
            else if (direction.y != 0)
                chain.AddVertical(centerCoord);
        }

        private readonly List<Token> currentlyClearedTokens = new List<Token>();
        private void ClearChainTokens(TokensChain chain)
        {
            Debug.Log(chain);
            int multiplier = Mathf.Max(1, chain.Size - 2);
            score += multiplier * chain.Size;
            foreach (var tokenCoord in chain.TokenCoords)
            {
                var token = boardController.Board.GetToken(tokenCoord);
                currentlyClearedTokens.Add(token);
                token.OnCleared += Token_OnCleared;
                if (token.TryGetComponent<TokenClearingBehavior>(out var clearing))
                {
                    clearing.ClearToken();
                }
                else
                {
                    token.IsCleared = true;
                }
            }
            OnScoreChanged?.Invoke(score);
        }

        private void Token_OnCleared(Token token)
        {
            token.OnCleared -= Token_OnCleared;
            currentlyClearedTokens.Remove(token);
            if (currentlyClearedTokens.Count <= 0)
                boardController.Collapse();
        }

        private void OnDisable()
        {
            boardController.OnTokensColapsed -= BoardController_OnTokensColapsed;
            swapManager.OnSwapRequested -= SwapManager_OnSwapRequested;
        }
    }
}
