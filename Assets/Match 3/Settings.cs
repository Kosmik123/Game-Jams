using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Bipolar.Match3
{
    [CreateAssetMenu(menuName = CreateAssetsPath.Root + "Settings")]
    public class Settings : ScriptableObject
    {
        [SerializeField]
        private TokenType[] possibleTokenTypes;
        public IReadOnlyList<TokenType> TokenTypes => possibleTokenTypes;

        public TokenType GetTokenType()
        {
            return TokenTypes[Random.Range(0, TokenTypes.Count)];
        }

        private readonly List<TokenType> tempAvailableTypes = new List<TokenType>();
        public TokenType GetTokenTypeExcept(IReadOnlyList<TokenType> exceptions)
        {
            tempAvailableTypes.Clear();
            foreach (var type in TokenTypes)
                if (exceptions.Contains(type) == false)
                    tempAvailableTypes.Add(type);

            return tempAvailableTypes[Random.Range(0, tempAvailableTypes.Count)];
        }
    }
}
