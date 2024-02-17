using System.Collections.Generic;
using UnityEngine;

namespace Bipolar.Match3
{
    public class RectangularBoardTokensMovementManager : TokensMovementManager
    {
        public override event System.Action OnTokensMovementStopped;

        [SerializeField]
        private RectangularBoard board;

        private readonly List<TokenMovement> currentlyMovingTokens = new List<TokenMovement>();
        public override bool AreTokensMoving => currentlyMovingTokens.Count > 0;

        public void StartTokenMovement(Token token, Vector2Int targetCoord, float duration = -1) => StartTokenMovement(token, targetCoord.x, targetCoord.y, duration);
        public void StartTokenMovement(Token token, int xTargetCoord, int yTargetCoord, float duration = -1)
        {
            if (token.TryGetComponent<TokenMovement>(out var tokenMovement))
            {
                tokenMovement.OnMovementEnded += Token_OnMovementEnded;
                tokenMovement.BeginMovingToPosition(board.CoordToWorld(xTargetCoord, yTargetCoord), duration);
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
