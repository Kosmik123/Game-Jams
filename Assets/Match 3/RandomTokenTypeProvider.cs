using UnityEngine;

namespace Bipolar.Match3
{

    public class RandomTokenTypeProvider : TokenTypeProvider
    {
        [SerializeField]
        private Settings settings;

        public override TokenType GetTokenType(int x, int y)
        {
            return settings.GetTokenType();
        }
    }
}
