using UnityEngine;

namespace Bipolar.Match3
{
    public abstract class TokenTypeProvider : MonoBehaviour
    {
        public abstract TokenType GetTokenType(int x, int y);
    }
}
