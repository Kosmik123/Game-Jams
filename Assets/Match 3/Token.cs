using JetBrains.Annotations;
using System.Collections;
using UnityEngine;

namespace Bipolar.Match3
{
    public class Token : MonoBehaviour
    {
        public event System.Action<Token> OnMovementEnded;
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

        private const float defaultMoveTime = 0.5f;
        public void BeginMovingToPosition(Vector3 position, float moveDuration = -1)
        {
            StopAllCoroutines();
            float moveTime = moveDuration < 0 ? defaultMoveTime : moveDuration;
            StartCoroutine(MovementCo(position, moveTime));
        }

        private IEnumerator MovementCo(Vector3 position, float duration)
        {
            Vector3 startPosition = transform.position;
            Vector3 targetPosition = position;
            float moveProgress = 0;
            float progressSpeed = 1f / duration;
            while (moveProgress < 1)
            {
                moveProgress += progressSpeed * Time.deltaTime;
                transform.position = Vector3.Lerp(startPosition, targetPosition, moveProgress);
                yield return null;
            }
            transform.position = targetPosition;
            OnMovementEnded?.Invoke(this);
        }

        void OnDestroy()
        {
            OnDestroyed?.Invoke();
        }

        private void OnValidate()
        {
            Type = type;
        }
    }
}
