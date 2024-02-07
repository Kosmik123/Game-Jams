using System.Collections;
using UnityEngine;

namespace Bipolar.Match3
{
    [RequireComponent(typeof(Token))]
    public class TokenMovement : MonoBehaviour
    {
        public event System.Action<TokenMovement> OnMovementEnded;

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

        [SerializeField]
        private float defaultMoveTime = 0.5f;
        
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

    }
}
