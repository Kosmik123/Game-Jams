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
                    coordsToCheck.Clear();
                    Vector2Int coord = new Vector2Int(i, j);
                    if (tokenChains.FirstOrDefault(chain => chain.Contains(coord)) != null)
                        continue;
                    
                    var chain = new TokensChain(boardController.Board.GetToken(coord).Type);
                    FindMatches(chain, coord);
                    if (chain.IsMatchFound)
                        tokenChains.Add(chain);
                }
            }

            foreach (var chain in tokenChains)
                ClearChainTokens(chain);

            return tokenChains.Count > 0;
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

        private static readonly Vector2Int[] chainsDirections =
        {
            Vector2Int.right,
            Vector2Int.up,
            Vector2Int.left,
            Vector2Int.down
        };

        private bool FindMatches(TokensChain chain, Vector2Int tokenCoord, Vector2Int exception = default, bool twoSidedException = true)
        {
            if (chain.Contains(tokenCoord))
                return false;

            chain.Add(tokenCoord);
            foreach (var direction in chainsDirections)
            {
                if (direction != exception && (twoSidedException == false || direction != -exception))
                    PopulateChain(chain, tokenCoord, direction);
            }

            return true;
        }

        private bool PopulateChain(TokensChain chain, Vector2Int tokenCoord, Vector2Int direction)
        {
            var nearCoord = tokenCoord + direction;
            if (boardController.Board.Contains(nearCoord) == false)
                return false;

            var nearToken = boardController.Board.GetToken(nearCoord);
            if (chain.TokenType != nearToken.Type)
                return false;

            bool isChainInThisDirection = false;

            var furtherCoord = nearCoord;
            do
            {
                furtherCoord += direction;
                var furtherToken = boardController.Board.GetToken(furtherCoord);
                if (furtherToken && furtherToken.Type == chain.TokenType)
                {
                    isChainInThisDirection = true;
                    Debug.Log($"Adding h/v going forward ending at {furtherCoord}");
                    if (direction.x != 0)
                        chain.HorizontalLinesCount++;
                    else if (direction.y != 0)
                        chain.VerticalLinesCount++;
                    chain.IsMatchFound = true;

                    FindMatches(chain, nearCoord, direction);
                    FindMatches(chain, furtherCoord, direction);
                }
                else
                    break;
            }
            while (true);

            if (isChainInThisDirection == false)
            {
                var backCoord = tokenCoord - direction;
                var backToken = boardController.Board.GetToken(backCoord);
                if (backToken && backToken.Type == chain.TokenType)
                {
                    isChainInThisDirection = true;
                    Debug.Log($"Adding h/v going back: {nearCoord}-{tokenCoord}-{backCoord}");
                    if (direction.x != 0)
                        chain.HorizontalLinesCount++;
                    else if (direction.y != 0)
                        chain.VerticalLinesCount++;
                    chain.IsMatchFound = true;

                    FindMatches(chain, nearCoord, direction);
                    if (FindMatches(chain, backCoord, direction))
                    {

                    }
                }
            }


            return isChainInThisDirection;
        }

        private void OnDisable()
        {
            boardController.OnTokensColapsed -= BoardController_OnTokensColapsed;
            swapManager.OnSwapRequested -= SwapManager_OnSwapRequested;
        }
    }
}
