using System.Collections.Generic;
using UnityEngine;

namespace Bipolar.Match3
{
    public class NonAdjacentTokenTypeProvider : TokenTypeProvider
    {
        [SerializeField]
        private Settings settings;
        [SerializeField]
        private Board board;

        private readonly HashSet<TokenType> forbiddenTokens = new HashSet<TokenType>();
        
        public override TokenType GetTokenType(int x, int y)
        {
            forbiddenTokens.Clear();
            Token token = board.GetToken(x - 1, y);
            if (token)
                forbiddenTokens.Add(token.Type);

            token = board.GetToken(x + 1, y);
            if (token)
                forbiddenTokens.Add(token.Type);

            token = board.GetToken(x, y - 1);
            if (token)
                forbiddenTokens.Add(token.Type);

            token = board.GetToken(x, y + 1);
            if (token)
                forbiddenTokens.Add(token.Type);

            return settings.GetTokenTypeExcept(forbiddenTokens);
        }
    }
}
