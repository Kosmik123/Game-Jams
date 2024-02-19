using UnityEngine;

namespace Bipolar.Match3
{
    [RequireComponent(typeof(GeneralBoard))]
    public class GeneralBoardCollapsing : BoardCollapsing<GeneralBoard>
    {
        public override event System.Action OnPiecesColapsed;

        private GeneralBoardPiecesMovementManager piecesMovementManager;

        public void Init (GeneralBoardPiecesMovementManager movementManager)
        {
            piecesMovementManager = movementManager;
        }

        public override void Collapse()
        {
            bool colapsed = false;
            foreach (var line in Board.Lines)
            {
                int emptyCellsCount = CollapseTokensInLine(line);
                if (emptyCellsCount > 0)
                {
                    colapsed = true;
                    RefillLine(line, emptyCellsCount);
                }
            }

            if (colapsed)
                piecesMovementManager.OnPiecesMovementStopped += CallCollapseEvent;
        }

        private int CollapseTokensInLine(GeneralBoard.CoordsLine line)
        {
            int nonExistingTokensCount = 0;
            for (int index = line.Coords.Count - 1; index >= 0; index--)
            {
                var coord = line.Coords[index];
                var token = Board.GetPiece(coord);
                if (token == null || token.IsCleared)
                {
                    nonExistingTokensCount++;
                }
                else if (nonExistingTokensCount > 0)
                {
                    piecesMovementManager.StartPieceMovement(token, line, index, nonExistingTokensCount);
                }
            }
            return nonExistingTokensCount;
        }

        private void RefillLine(GeneralBoard.CoordsLine line, int count)
        {
            var startCoord = line.Coords[0];
            var creatingDirection = -Board.GetRealDirection(startCoord);
            var firstCellPosition = Board.CoordToWorld(startCoord);
            for (int i = 0; i < count; i++)
            {
                var coord = line.Coords[i];
                var newPiece = CreatePiece(coord);

                var spawningPosition = firstCellPosition + (Vector3)(creatingDirection * (count - i));
                newPiece.transform.position = spawningPosition;
                piecesMovementManager.StartPieceMovement(newPiece, line, -1, i + 1);
            }
        }

        private void CallCollapseEvent()
        {
            piecesMovementManager.OnPiecesMovementStopped -= CallCollapseEvent;
            OnPiecesColapsed?.Invoke();
        }
    }
}
