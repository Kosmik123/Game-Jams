using System.Collections.Generic;
using UnityEngine;

namespace Bipolar.Match3
{
    public abstract class Matcher : MonoBehaviour
    {
        private static readonly Vector2Int[] chainsDirections =
        {
            Vector2Int.right,
            Vector2Int.up,
            Vector2Int.left,
            Vector2Int.down
        };

        protected readonly List<PiecesChain> tokenChains = new List<PiecesChain>();
        public IReadOnlyList<PiecesChain> PieceChains => tokenChains;

        public abstract void FindAndCreatePieceChains(Board board);

        protected void CreateTokensChain(Board board, Vector2Int coord, Queue<Vector2Int> coordsToCheck = null)
        {
            coordsToCheck ??= new Queue<Vector2Int>();
            coordsToCheck.Clear();
            coordsToCheck.Enqueue(coord);
            var chain = new TriosTokensChain();
            chain.TokenType = board.GetPiece(coord).Type;
            FindMatches(board, chain, coordsToCheck);

            if (chain.IsMatchFound)
                tokenChains.Add(chain);
        }

        public static void FindMatches(Board board, TriosTokensChain chain, Queue<Vector2Int> coordsToCheck)
        {
            while (coordsToCheck.Count > 0)
            {
                var tokenCoord = coordsToCheck.Dequeue();
                chain.Add(tokenCoord);
                foreach (var direction in chainsDirections)
                {
                    TryAddLineToChain(board, chain, tokenCoord, direction, coordsToCheck);
                }
            }
        }

        public static bool TryAddLineToChain(Board board, TriosTokensChain chain, Vector2Int tokenCoord, Vector2Int direction, Queue<Vector2Int> coordsToCheck)
        {
            var nearCoord = tokenCoord + direction;
            var nearToken = board.GetPiece(nearCoord);
            if (nearToken == null || chain.TokenType != nearToken.Type)
                return false;

            var backCoord = tokenCoord - direction;
            var backToken = board.GetPiece(backCoord);
            if (backToken && chain.TokenType == backToken.Type)
            {
                chain.IsMatchFound = true;
                TryEnqueueCoord(chain, coordsToCheck, nearCoord);
                TryEnqueueCoord(chain, coordsToCheck, backCoord);
                AddLineToChain(chain, tokenCoord, direction);
                return true;
            }

            var furtherCoord = nearCoord + direction;
            var furtherToken = board.GetPiece(furtherCoord);
            if (furtherToken && chain.TokenType == furtherToken.Type)
            {
                chain.IsMatchFound = true;
                TryEnqueueCoord(chain, coordsToCheck, nearCoord);
                TryEnqueueCoord(chain, coordsToCheck, furtherCoord);
                AddLineToChain(chain, nearCoord, direction);
                return true;
            }

            return false;
        }

        public static void AddLineToChain(TriosTokensChain chain, Vector2Int centerCoord, Vector2Int direction)
        {
            if (direction.x != 0)
                chain.AddHorizontal(centerCoord);
            else if (direction.y != 0)
                chain.AddVertical(centerCoord);
        }

        public static bool TryEnqueueCoord(PiecesChain chain, Queue<Vector2Int> coordsToCheck, Vector2Int coord)
        {
            if (chain.Contains(coord))
                return false;

            if (coordsToCheck.Contains(coord))
                return false;

            coordsToCheck.Enqueue(coord);
            return true;
        }
    }
}
