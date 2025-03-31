using UnityEngine;

namespace Bipolar.Breakout
{
    public class Paddle : MonoBehaviour
    {
        [field: SerializeField]
        public PaddleMovement Movement { get; private set; }
        
        [field: SerializeField]
        public PaddleCollision Collision { get; private set; }

        [field: SerializeField]
        public Transform BallOrigin { get; private set; }
    }
}
