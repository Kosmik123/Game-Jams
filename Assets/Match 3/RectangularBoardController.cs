﻿using System.Collections.Generic;
using UnityEngine;

namespace Bipolar.Match3
{
    [RequireComponent(typeof(RectangularBoard))]
    public class RectangularBoardController : BoardController<RectangularBoard>
    {
        public override event System.Action OnTokensMovementStopped;

        public event System.Action OnTokensColapsed;
        public event TokensSwapEventHandler OnTokensSwapped;
        
        private readonly List<TokenMovement> currentlyMovingTokens = new List<TokenMovement>();
        public bool AreTokensMoving => currentlyMovingTokens.Count > 0;

        public override void Collapse()
        {
            int iterationAxis = (board.CollapseDirection.x != 0) ? 1 : 0;
            bool colapsed = false;
            for (int lineIndex = 0; lineIndex < board.Dimensions[iterationAxis]; lineIndex++)
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

        private void CallCollapseEvent()
        {
            OnTokensMovementStopped -= CallCollapseEvent;
            OnTokensColapsed?.Invoke();
        }

        private int CollapseTokensInLine(int lineIndex, int iterationAxis)
        {
            int collapseAxis = 1 - iterationAxis; // to samo
            int lineSize = board.Dimensions[collapseAxis]; // to samo

            int startCellIndex = board.CollapseDirection[collapseAxis] > 0 ? -1 : 0; // odwrócony warunek
            int lineCollapseDirection = board.CollapseDirection[collapseAxis] == 0 ? 1 : -board.CollapseDirection[collapseAxis];

            int nonExistingTokensCount = 0; // inna rzecz
            for (int i = 0; i < lineSize; i++) // troszkę inne
            {
                var coord = Vector2Int.zero; // to samo
                coord[iterationAxis] = lineIndex; // to samo
                coord[collapseAxis] = (startCellIndex + i * lineCollapseDirection + lineSize) % lineSize; // odwrócony znak

                // odtąd są inne czynnności
                var token = Board.GetToken(coord);
                if (token == null || token.IsCleared)
                {
                    nonExistingTokensCount++;
                }
                else if (nonExistingTokensCount > 0)
                {
                    var offsetToMove = board.CollapseDirection * nonExistingTokensCount;
                    var targetCoord = coord + offsetToMove;
                    Board[coord] = null;
                    Board[targetCoord] = token;
                    StartTokenMovement(token, targetCoord, 0.3f); // to samo
                }
            }

            return nonExistingTokensCount;
        }

        private void RefillLine(int lineIndex, int count, int iterationAxis)
        {
            int collapseAxis = 1 - iterationAxis; // to samo
            int lineSize = board.Dimensions[collapseAxis]; // to samo

            int startCellIndex = board.CollapseDirection[collapseAxis] < 0 ? -1 : 0; // odwrócony warunek
            var spawnOffset = -board.CollapseDirection * count; // inna rzecz
            
            int refillingDirection = board.CollapseDirection[collapseAxis] == 0 ? 1 : board.CollapseDirection[collapseAxis];

            for (int i = 0; i < count; i++) 
            {
                var coord = Vector2Int.zero; // to samo
                coord[iterationAxis] = lineIndex; // to samo
                coord[collapseAxis] = (startCellIndex + i * refillingDirection + lineSize) % lineSize; // odwrócony znak

                // odtąd inne czynnności
                var spawnCoord = coord + spawnOffset;
                var newToken = CreateToken(coord.x, coord.y);
                newToken.transform.position = Board.CoordToWorld(spawnCoord);
                Board[coord] = newToken;
                StartTokenMovement(newToken, coord, 0.3f); // to samo
            }
        }

        private System.Action swapEndedCallback;
        public void SwapTokens(Vector2Int tokenCoord1, Vector2Int tokenCoord2)
        {
            var token1 = Board.GetToken(tokenCoord1);
            var token2 = Board.GetToken(tokenCoord2);

            StartTokenMovement(token2, tokenCoord1);
            StartTokenMovement(token1, tokenCoord2);


            var temp = Board[tokenCoord1];
            Board[tokenCoord1] = Board[tokenCoord2];
            Board[tokenCoord2] = temp;

            //(Board[tokenCoord1], Board[tokenCoord2]) = (Board[tokenCoord2], Board[tokenCoord1]);

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