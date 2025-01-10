using UnityEngine;

namespace Bipolar.Breakout
{
    public class MultipleHitpontsBrick : BrickHitBehavior
    {
        [SerializeField]
        private int hitpoints = 2;

        public override void ProcessHit(Collision2D collision)
        {
            hitpoints--;
            if (hitpoints <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
