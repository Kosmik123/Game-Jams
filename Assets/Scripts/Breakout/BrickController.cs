using UnityEngine;

namespace Bipolar.Breakout
{
    public delegate void BrickBreakEventHandler(int points);

    public class BrickController : MonoBehaviour
    {
        public static event BrickBreakEventHandler OnBrickBroke;

        [SerializeField]
        private BrickHitBehavior customBehavior;
        [SerializeField]
        private int pointsValue;

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
                    OnBrickBroke?.Invoke(pointsValue);
                }
            }
        }

    }

    public abstract class BrickHitBehavior : MonoBehaviour
    {
        public abstract void ProcessHit(Collision2D collision);
    }
}
