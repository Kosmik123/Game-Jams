using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Bipolar.Match3
{
    public class Matcher : MonoBehaviour
    {
        private static readonly Vector2Int[] chainsDirections =
        {
            Vector2Int.right,
            Vector2Int.up,
            Vector2Int.left,
            Vector2Int.down
        };

        private readonly List<TokensChain> tokenChains = new List<TokensChain>();
        public IReadOnlyList<TokensChain> TokenChains => tokenChains;

        private readonly Queue<Vector2Int> coordsToCheck = new Queue<Vector2Int>();

        public void FindAndCreateTokenChains(RectangularBoard board)
        {
            tokenChains.Clear();
            for (int j = 0; j < board.Dimensions.y; j++)
            {
                for (int i = 0; i < board.Dimensions.x; i++)
                {
                    Vector2Int coord = new Vector2Int(i, j);
                    if (tokenChains.FirstOrDefault(chain => chain.Contains(coord)) != null)
                        continue;

                    coordsToCheck.Clear();
                    coordsToCheck.Enqueue(coord);
                    var chain = new TriosTokensChain();
                    chain.TokenType = board.GetToken(coord).Type;
                    FindMatches(board, chain, coordsToCheck);

                    if (chain.IsMatchFound)
                        tokenChains.Add(chain);
                }
            }
        }

        private void FindMatches(RectangularBoard board, TriosTokensChain chain, Queue<Vector2Int> coordsToCheck)
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

        private bool TryAddLineToChain(RectangularBoard board, TriosTokensChain chain, Vector2Int tokenCoord, Vector2Int direction, Queue<Vector2Int> coordsToCheck)
        {
            var nearCoord = tokenCoord + direction;
            var nearToken = board.GetToken(nearCoord);
            if (nearToken == null || chain.TokenType != nearToken.Type)
                return false;

            var backCoord = tokenCoord - direction;
            var backToken = board.GetToken(backCoord);
            if (backToken && chain.TokenType == backToken.Type)
            {
                chain.IsMatchFound = true;
                TryEnqueueCoord(chain, coordsToCheck, nearCoord);
                TryEnqueueCoord(chain, coordsToCheck, backCoord);
                AddLineToChain(chain, tokenCoord, direction);
                return true;
            }

            var furtherCoord = nearCoord + direction;
            var furtherToken = board.GetToken(furtherCoord);
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

        private void AddLineToChain(TriosTokensChain chain, Vector2Int centerCoord, Vector2Int direction)
        {
            if (direction.x != 0)
                chain.AddHorizontal(centerCoord);
            else if (direction.y != 0)
                chain.AddVertical(centerCoord);
        }

        private static bool TryEnqueueCoord(TokensChain chain, Queue<Vector2Int> coordsToCheck, Vector2Int coord)
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
