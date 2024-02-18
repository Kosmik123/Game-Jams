using UnityEngine;

namespace Bipolar.Match3
{
    [RequireComponent(typeof(GeneralBoard))]
    public class GeneralBoardController : BoardController<GeneralBoard>
    {
        public override event System.Action OnTokensColapsed;
        public override event TokensSwapEventHandler OnTokensSwapped;

        [SerializeField]
        private GeneralBoardTokensMovementManager tokensMovementManager;
        public override bool AreTokensMoving => tokensMovementManager.AreTokensMoving;

        private void Start()
        {
            Collapse();
        }

        public override void Collapse()
        {
            bool colapsed = false;
            foreach (var line in board.Lines)
            {
                int emptyCellsCount = CollapseTokensInLine(line);
                if (emptyCellsCount > 0)
                {
                    colapsed = true;
                    RefillLine(line, emptyCellsCount);
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

        private int CollapseTokensInLine(GeneralBoard.CoordsLine line)
        {
            int nonExistingTokensCount = 0;
            for (int index = 0; index < line.Coords.Count; index++)
            {
                var coord = line.Coords[index];
                var token = Board.GetToken(coord);
                if (token == null || token.IsCleared)
                {
                    nonExistingTokensCount++;
                }
                else if (nonExistingTokensCount > 0)
                {
                    tokensMovementManager.StartTokenMovement(token, line, index, nonExistingTokensCount);
                }
            }
            return nonExistingTokensCount;
        }

        private void RefillLine(GeneralBoard.CoordsLine line, int count)
        {
            var startCoord = line.Coords[0];
            var creatingDirection = -board.GetRealDirection(startCoord);
            var firstCellPosition = board.CoordToWorld(startCoord);
            for (int i = 0; i < count; i++)
            {
                var coord = line.Coords[i];
                var newToken = CreateToken(coord.x, coord.y);
                var spawningPosition = firstCellPosition + (Vector3)(creatingDirection * (count - i));
                newToken.transform.position = spawningPosition;
                tokensMovementManager.StartTokenMovement(newToken, line, -1, i + 1);
            }
        }

        public override void SwapTokens(Vector2Int tokenCoord1, Vector2Int tokenCoord2)
        {
            (Board[tokenCoord1], Board[tokenCoord2]) = (Board[tokenCoord2], Board[tokenCoord1]);
            
            var token1 = Board.GetToken(tokenCoord1);
            var token2 = Board.GetToken(tokenCoord2);
            token2.transform.position = Board.CoordToWorld(tokenCoord1);
            token1.transform.position = Board.CoordToWorld(tokenCoord2);

            OnTokensSwapped(tokenCoord1, tokenCoord2);
        }
    }
}
