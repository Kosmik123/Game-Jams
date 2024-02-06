using System.Collections.Generic;
using UnityEngine;

namespace Bipolar.Match3
{
    [CreateAssetMenu(menuName = CreateAssetsPath.Root + "Settings")]
    public class Settings : ScriptableObject
    {
        [SerializeField]
        private TokenType[] possibleTokenTypes;
        public IReadOnlyList<TokenType> TokenTypes => possibleTokenTypes;
    }
}
