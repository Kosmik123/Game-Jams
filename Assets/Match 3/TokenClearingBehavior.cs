using UnityEngine;

namespace Bipolar.Match3
{
    [RequireComponent(typeof(Token))]
    public abstract class TokenClearingBehavior : MonoBehaviour
    {
        private Token _token;
        public Token Token
        {
            get
            {
                if (_token == null)
                    _token = GetComponent<Token>();
                return _token;
            }
        }

        public abstract void ClearToken();

        protected void FinishClearing()
        {
            Token.IsCleared = true;
        }
    }
}
