using System.Collections.Generic;
using UnityEngine;

namespace Bipolar.Match3
{
    [RequireComponent(typeof(GeneralBoard))]
    public class GeneralBoardController : BoardController<GeneralBoard>
    {
        [SerializeField]
        private GeneralBoardTokensMovementManager tokensMovementManager;

        private readonly List<TokenMovement> currentlyMovingTokens = new List<TokenMovement>();
        public bool AreTokensMoving => currentlyMovingTokens.Count > 0;

        private readonly Dictionary<Token, Coroutine> tokenMovementCoroutines = new Dictionary<Token, Coroutine>();

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
    }
}
