using UnityEngine;

namespace Bipolar.Match3
{
    public class Token : MonoBehaviour
    {
        public event System.Action<TokenType> OnTypeChanged;
        public event System.Action OnDestroyed;

        [SerializeField]
        private TokenType type;
        public TokenType Type
        {
            get => type;
            set
            {
                type = value;
                OnTypeChanged?.Invoke(type);
            }
        }

        public bool IsDestroyed { get; set; } = false;

        void OnDestroy()
        {
            OnDestroyed?.Invoke();
        }

        private void OnValidate()
        {
            Type = type;
        }
    }

    [RequireComponent(typeof(Token))]
    public abstract class TokenDestroyBehavior : MonoBehaviour
    {
        public abstract void DestroyToken();
    }


}
