using UnityEngine;

namespace Bipolar.Match3
{
    public class Token : MonoBehaviour
    {
        public event System.Action<TokenType> OnTypeChanged;
        public event System.Action<Token> OnCleared;

        [SerializeField]
        private TokenType type;
        public TokenType Type
        {
            get => type;
            set
            {
                type = value;
                if (gameObject.scene.IsValid()) 
                    gameObject.name = $"Token ({type.name})";
                OnTypeChanged?.Invoke(type);
            }
        }

        private bool isCleared = false;
        public bool IsCleared
        {
            get => isCleared;
            set
            {
                isCleared = value;
                if (isCleared)
                    OnCleared?.Invoke(this);
            }
        }

        private void OnValidate()
        {
            Type = type;
        }
    }
}
