using Bipolar.PuzzleBoard;
using Bipolar.PuzzleBoard.General;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Bipolar.Match3
{
    public class GeneralBoardMatcher : Matcher
    {
        private readonly Queue<Vector2Int> coordsToCheck = new Queue<Vector2Int>();

        public override void FindAndCreatePieceChains(Board board) => FindAndCreateTokenChains((GeneralBoard)board);
        public void FindAndCreateTokenChains(GeneralBoard board)
        {
            pieceChains.Clear();
            foreach (var coord in board.Coords)
            {
                if (pieceChains.FirstOrDefault(chain => chain.Contains(coord)) != null)
                    continue;

                CreateTokensChain(board, coord, coordsToCheck);
            }
        }
    }
}
