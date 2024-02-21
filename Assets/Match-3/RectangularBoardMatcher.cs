using Bipolar.PuzzleBoard;
using Bipolar.PuzzleBoard.Rectangular;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Bipolar.Match3
{
    public class RectangularBoardMatcher : Matcher
    {
        private readonly Queue<Vector2Int> coordsToCheck = new Queue<Vector2Int>();

        public override void FindAndCreatePieceChains(Board board) => FindAndCreateTokenChains((RectangularBoard) board);
        public void FindAndCreateTokenChains(RectangularBoard board)
        {
            pieceChains.Clear();
            for (int j = 0; j < board.Dimensions.y; j++)
            {
                for (int i = 0; i < board.Dimensions.x; i++)
                {
                    Vector2Int coord = new Vector2Int(i, j);
                    if (pieceChains.FirstOrDefault(chain => chain.Contains(coord)) != null)
                        continue;

                    CreateTokensChain(board, coord, coordsToCheck);
                }
            }
        }
    }
}
