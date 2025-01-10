using UnityEngine;

namespace Bipolar.Breakout
{
    public class BrickController : MonoBehaviour
    {
        [SerializeField]
        private BrickHitBehavior customBehavior;

        private void OnCollisionEnter2D(Collision2D collision)
        {
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
