using UnityEngine;

namespace Bipolar.Breakout
{
    public class BrickController : MonoBehaviour
    {
        [SerializeField]
        private BrickHitBehavior customBehavior;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            Debug.Log("Pi³ka ma tag: " + collision.gameObject.tag);
            Debug.Log("Tag w ustawieniach: " + GameSettings.BallTag);
            if (collision.gameObject.CompareTag(GameSettings.BallTag))
            {
                if (customBehavior)
                {
                    customBehavior.ProcessHit(collision);
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }

    }

    public abstract class BrickHitBehavior : MonoBehaviour
    {
        public abstract void ProcessHit(Collision2D collision);
    }
}
