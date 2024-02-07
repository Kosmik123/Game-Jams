using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Bipolar.Match3
{
    public class MatchManager : MonoBehaviour
    {
        [SerializeField]
        private BoardController boardController;
        [SerializeField]
        private SwapManager swapManager;

        private void OnEnable()
        {
            boardController.OnTokensColapsed += BoardController_OnTokensColapsed;
            swapManager.OnSwapRequested += SwapManager_OnSwapRequested;
        }

        private void SwapManager_OnSwapRequested(Vector2Int tokenCoord1, Vector2Int tokenCoord2)
        {
            if (boardController.AreTokensMoving == false)
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

        private bool FindMatches()
        {
            tokenChains.Clear();
            for (int j = 0; j < boardController.Board.Dimentions.y; j++)
            {
                for (int i = 0; i < boardController.Board.Dimentions.x; i++)
                {
                    Vector2Int coord = new Vector2Int(i, j);
                    var chain = new TokensChain(boardController.Board.GetToken(coord).Type);
                    FindMatches(chain, coord);
                    if (chain.IsMatchFound)
                        if (tokenChains.FirstOrDefault(chain => chain.Contains(coord)) == null)
                            tokenChains.Add(chain);
                }
            }

            foreach (var chain in tokenChains)
            {
                DestroyChainTokens(chain);
            }

            boardController.Collapse();
            return tokenChains.Count > 0;
        }

        private void DestroyChainTokens(TokensChain chain)
        {
            foreach (var tokenCoord in chain.TokenCoords)
            {
                var token = boardController.Board.GetToken(tokenCoord);
                Destroy(token.gameObject);
                token.IsDestroyed = true;
            }
        }

        private static readonly Vector2Int[] chainsDirections =
        {
            Vector2Int.right,
            Vector2Int.up,
            Vector2Int.left,
            Vector2Int.down
        };

        private void FindMatches(TokensChain chain, Vector2Int tokenCoord)
        {
            if (chain.Contains(tokenCoord))
                return;

            chain.Add(tokenCoord);
            foreach (var direction in chainsDirections)
            {
                PopulateChain(chain, tokenCoord, direction);
            }
        }

        private void PopulateChain(TokensChain chain, Vector2Int tokenCoord, Vector2Int direction)
        {
            var nearCoord = tokenCoord + direction;
            if (boardController.Board.Contains(nearCoord) == false)
                return;

            var nearToken = boardController.Board.GetToken(nearCoord);
            if (chain.TokenType != nearToken.Type)
                return;

            bool isChainInThisDirection = false;
            var furtherCoord = nearCoord + direction;
            var furtherToken = boardController.Board.GetToken(furtherCoord);
            if (furtherToken && furtherToken.Type == chain.TokenType)
            {
                isChainInThisDirection = true;
                chain.IsMatchFound = true;
                FindMatches(chain, furtherCoord);
            }

            var backCoord = tokenCoord - direction;
            var backToken = boardController.Board.GetToken(backCoord);
            if (backToken && backToken.Type == chain.TokenType)
            {
                isChainInThisDirection = true;
                chain.IsMatchFound = true;
                FindMatches(chain, backCoord);
            }

            if (isChainInThisDirection)
            {
                FindMatches(chain, nearCoord);
            }
        }

        private void OnDisable()
        {
            boardController.OnTokensColapsed -= BoardController_OnTokensColapsed;
            swapManager.OnSwapRequested -= SwapManager_OnSwapRequested;
        }
    }
}
