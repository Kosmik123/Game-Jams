using System.Collections.Generic;
using UnityEngine;

namespace Bipolar.Match3
{
    public delegate void TokensSwapEventHandler(Vector2Int tokenCoord1, Vector2Int tokenCoord2);

    public class MatchManager : MonoBehaviour
    {
        public event System.Action OnMatchingFailed;
        public event System.Action<TokensChain> OnTokensMatched;

        [SerializeField]
        private BoardController boardController;
        [SerializeField]
        private SwapManager swapManager;

        [SerializeField]
        private Matcher matcher;

        [SerializeField]
        private int combo;
        public int Combo => combo;

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
            combo = 0;
            boardController.OnTokensSwapped += BoardController_OnTokensSwapped;
            boardController.SwapTokens(tokenCoord1, tokenCoord2);
        }

        private void BoardController_OnTokensSwapped(Vector2Int tokenCoord1, Vector2Int tokenCoord2)
        {
            boardController.OnTokensSwapped -= BoardController_OnTokensSwapped;
            FindMatches();
            bool wasCorrectMove = matcher.TokenChains.Count > 0;
            if (wasCorrectMove == false)
            {
                boardController.SwapTokens(tokenCoord1, tokenCoord2);
                OnMatchingFailed?.Invoke();
            }
        }

        private void BoardController_OnTokensColapsed()
        {
            FindMatches();
        }

        private void FindMatches()
        {
            matcher.FindAndCreateTokenChains(boardController.Board);

            combo += matcher.TokenChains.Count;
            foreach (var chain in matcher.TokenChains)
            {
                OnTokensMatched?.Invoke(chain);
                ClearChainTokens(chain);
            }
        }

        private readonly List<Token> currentlyClearedTokens = new List<Token>();
        private void ClearChainTokens(TokensChain chain)
        {
            foreach (var coord in chain.TokenCoords)
            {
                var token = boardController.Board.GetToken(coord);
                currentlyClearedTokens.Add(token);
                token.OnCleared += Token_OnCleared;
                boardController.Board[coord] = null;
                if (token.TryGetComponent<TokenClearingBehavior>(out var clearing))
                {
                    clearing.ClearToken();
                }
                else
                {
                    token.IsCleared = true;
                }
            }
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
