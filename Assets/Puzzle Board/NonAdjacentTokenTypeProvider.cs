using System.Collections.Generic;
using UnityEngine;

namespace Bipolar.PuzzleBoard
{
    public class NonAdjacentTokenTypeProvider : PieceTypeProvider
    {
        [SerializeField]
        private Settings settings;
        [SerializeField]
        private Board board;

        private readonly HashSet<PieceType> forbiddenTokens = new HashSet<PieceType>();
        
        public override PieceType GetPieceType(int x, int y)
        {
            forbiddenTokens.Clear();
            Piece token = board.GetPiece(x - 1, y);
            if (token)
                forbiddenTokens.Add(token.Type);

            token = board.GetPiece(x + 1, y);
            if (token)
                forbiddenTokens.Add(token.Type);

            token = board.GetPiece(x, y - 1);
            if (token)
                forbiddenTokens.Add(token.Type);

            token = board.GetPiece(x, y + 1);
            if (token)
                forbiddenTokens.Add(token.Type);

            return settings.GetPieceTypeExcept(forbiddenTokens);
        }
    }
}
