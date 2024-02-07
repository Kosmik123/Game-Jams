﻿using System;
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
        private TokensClickDetector tokensClickDetector;
        [SerializeField]
        private SwipeDetector swipeDetector;

        [SerializeField]
        private Token selectedToken;

        private void OnEnable()
        {
            swipeDetector.OnTokenSwiped += SwipeDetector_OnTokenSwiped;
            tokensClickDetector.OnTokenClicked += TokensClickDetector_OnTokenClicked;
            boardController.OnTokensColapsed += BoardController_OnTokensColapsed;
        }

        private void TokensClickDetector_OnTokenClicked(Vector2Int tokenCoord)
        {
            selectedToken = boardController.Board.GetToken(tokenCoord);
        }

        private void SwipeDetector_OnTokenSwiped(Vector2Int tokenCoord, Vector2Int direction)
        {
            if (boardController.AreTokensMoving)
                return;

            var otherTokenCoord = tokenCoord + direction;
            if (boardController.Board.Contains(otherTokenCoord) == false)
                return;

            SwapTokens(tokenCoord, otherTokenCoord);
        }

        private void SwapTokens(Vector2Int token1Coord, Vector2Int token2Coord)
        {
            boardController.SwapTokens(token1Coord, token2Coord);
            boardController.OnTokensSwapped += BoardController_OnTokensSwapped;
        }

        private void BoardController_OnTokensSwapped(Vector2Int token1Coord, Vector2Int token2Coord)
        {
            boardController.OnTokensSwapped -= BoardController_OnTokensSwapped;
            FindMatches();
        }


        private List<TokensChain> tokenChains = new List<TokensChain>();
        private void BoardController_OnTokensColapsed()
        {
            FindMatches();
        }

        private void FindMatches()
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
            swipeDetector.OnTokenSwiped -= SwipeDetector_OnTokenSwiped;
            boardController.OnTokensColapsed -= BoardController_OnTokensColapsed;
            tokensClickDetector.OnTokenClicked -= TokensClickDetector_OnTokenClicked;
        }
    }

    public class TokensChain
    {
        public TokenType TokenType { get; private set; }
        private readonly HashSet<Vector2Int> tokenCoords = new HashSet<Vector2Int>();
        public IReadOnlyCollection<Vector2Int> TokenCoords => tokenCoords;
        public bool IsMatchFound { get; set; } = false;
        public bool Contains(Vector2Int tokenCoord) => tokenCoords.Contains(tokenCoord);
        public int Size => tokenCoords.Count;

        public TokensChain(TokenType type)
        {
            TokenType = type;
        }

        public void Add(Vector2Int tokenCoord)
        {
            tokenCoords.Add(tokenCoord);
        }

        public void Add(TokensChain chain)
        {
            tokenCoords.UnionWith(chain.TokenCoords);
        }

        public void Clear()
        {
            tokenCoords.Clear();
        }
    }
}
