using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Bipolar.Match3
{
    public interface ITokenTypeProvider
    {
        public TokenType GetTokenType();

    }

    [CreateAssetMenu(menuName = CreateAssetsPath.Root + "Settings")]
    public class Settings : ScriptableObject, ITokenTypeProvider
    {
        [SerializeField]
        private TokenType[] possibleTokenTypes;
        public IReadOnlyList<TokenType> TokenTypes => possibleTokenTypes;

        public TokenType GetTokenType()
        {
            return TokenTypes[Random.Range(0, TokenTypes.Count)];
        }

        public TokenType GetTokenTypeExcept(TokenType exception)
        {
            int index = Random.Range(1, TokenTypes.Count);
            if (TokenTypes[index] == exception)
                return TokenTypes[0];
            
            return TokenTypes[index];
        }

        private readonly List<TokenType> tempAvailableTypes = new List<TokenType>();
        public TokenType GetTokenTypeExcept(IEnumerable<TokenType> exceptions)
        {
            tempAvailableTypes.Clear();
            foreach (var type in TokenTypes)
                if (exceptions.Contains(type) == false)
                    tempAvailableTypes.Add(type);

            if (tempAvailableTypes.Count <= 0)
                return GetTokenType();

            return tempAvailableTypes[Random.Range(0, tempAvailableTypes.Count)];
        }
    }
}
