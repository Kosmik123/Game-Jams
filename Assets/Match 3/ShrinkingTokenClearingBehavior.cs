using System.Collections;
using UnityEngine;

namespace Bipolar.Match3
{
    public class ShrinkingTokenClearingBehavior : TokenClearingBehavior
    {
        [SerializeField]
        private float shrinkingDuration = 0.2f;
        [SerializeField]
        private Transform resizedVisual;
        [SerializeField]
        private Token token;

        private void OnEnable()
        {
            resizedVisual.localScale = Vector3.one;
        }

        public override void ClearToken()
        {
            StartCoroutine(ShrinkingCo());
        }

        private IEnumerator ShrinkingCo()
        {
            float progress = 0;
            float speed = 1f / shrinkingDuration;
            Vector3 initialScale = resizedVisual.localScale;
            while (progress < 1)
            {
                progress += Time.deltaTime * speed;
                resizedVisual.localScale = Vector3.Lerp(initialScale, Vector3.zero, progress);
                yield return null;
            }
            resizedVisual.localScale = Vector3.zero;
            token.IsCleared = true;
        }
    }
}
