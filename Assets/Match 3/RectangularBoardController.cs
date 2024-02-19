using UnityEngine;

namespace Bipolar.Match3
{
    [RequireComponent(typeof(RectangularBoard))]
    public class RectangularBoardController : BoardController<RectangularBoard>
    {
        public override event System.Action OnTokensColapsed;
        public override event TokensSwapEventHandler OnTokensSwapped;

        [SerializeField]
        private RectangularBoardTokensMovementManager tokensMovementManager;

        public override bool AreTokensMoving => tokensMovementManager.AreTokensMoving;

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
                tokensMovementManager.OnTokensMovementStopped += CallCollapseEvent;
        }

        private void CallCollapseEvent()
        {
            tokensMovementManager.OnTokensMovementStopped -= CallCollapseEvent;
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
                    tokensMovementManager.StartTokenMovement(token, targetCoord, 0.3f); // to samo
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
                var newToken = CreateToken(coord);
                var spawnCoord = coord + spawnOffset;
                newToken.transform.position = Board.CoordToWorld(spawnCoord);
                tokensMovementManager.StartTokenMovement(newToken, coord, 0.3f); // to samo
            }
        }

        private System.Action swapEndedCallback;
        public override void SwapTokens(Vector2Int tokenCoord1, Vector2Int tokenCoord2)
        {
            var token1 = Board.GetToken(tokenCoord1);
            var token2 = Board.GetToken(tokenCoord2);

            tokensMovementManager.StartTokenMovement(token2, tokenCoord1);
            tokensMovementManager.StartTokenMovement(token1, tokenCoord2);

            (Board[tokenCoord1], Board[tokenCoord2]) = (Board[tokenCoord2], Board[tokenCoord1]);

            tokensMovementManager.OnTokensMovementStopped += BoardController_OnTokensMovementStopped;
            swapEndedCallback = () =>
            {
                swapEndedCallback = null;
                OnTokensSwapped?.Invoke(tokenCoord1, tokenCoord2);
            };
        }

        private void BoardController_OnTokensMovementStopped()
        {
            tokensMovementManager.OnTokensMovementStopped -= BoardController_OnTokensMovementStopped;
            swapEndedCallback.Invoke();
        }
    }
}
