using UnityEngine;

namespace Bipolar.Match3
{
    public class TokenTypeProvider : MonoBehaviour
    {
        [SerializeField]
        private Settings settings;

        public TokenType GetTokenType(int x, int y)
        {
            return settings.GetTokenType();
        }
    }
}
