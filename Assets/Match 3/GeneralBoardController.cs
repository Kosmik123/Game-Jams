﻿using UnityEngine;

namespace Bipolar.Match3
{
    public class GeneralBoardCollapsing : BoardCollapsing<GeneralBoard>
    {
        public override void Collapse()
        {
            throw new System.NotImplementedException();
        }
    }

    [RequireComponent(typeof(GeneralBoard))]
    public class GeneralBoardController : BoardController<GeneralBoard, GeneralBoardCollapsing>
    {
        public override event System.Action OnPiecesColapsed;
        public override event PiecesSwapEventHandler OnPiecesSwapped;

        [SerializeField]
        private GeneralBoardPiecesMovementManager piecesMovementManager;
        public override bool ArePiecesMoving => piecesMovementManager.ArePiecesMoving;

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
                piecesMovementManager.OnPiecesMovementStopped += CallCollapseEvent;
        }

        private void CallCollapseEvent()
        {
            piecesMovementManager.OnPiecesMovementStopped -= CallCollapseEvent;
            OnPiecesColapsed?.Invoke();
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
            var creatingDirection = -board.GetRealDirection(startCoord);
            var firstCellPosition = board.CoordToWorld(startCoord);
            for (int i = 0; i < count; i++)
            {
                var coord = line.Coords[i];
                var newToken = CreatePiece(coord);

                var spawningPosition = firstCellPosition + (Vector3)(creatingDirection * (count - i));
                newToken.transform.position = spawningPosition;
                piecesMovementManager.StartPieceMovement(newToken, line, -1, i + 1);
            }
        }

        public override void SwapTokens(Vector2Int tokenCoord1, Vector2Int tokenCoord2)
        {
            (Board[tokenCoord1], Board[tokenCoord2]) = (Board[tokenCoord2], Board[tokenCoord1]);
            
            var token1 = Board.GetPiece(tokenCoord1);
            var token2 = Board.GetPiece(tokenCoord2);
            token2.transform.position = Board.CoordToWorld(tokenCoord1);
            token1.transform.position = Board.CoordToWorld(tokenCoord2);

            OnPiecesSwapped(tokenCoord1, tokenCoord2);
        }
    }
}
