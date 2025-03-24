using UnityEngine;

namespace Bipolar.Breakout
{
    public class PaddleController : MonoBehaviour
    {
        [field: SerializeField]
        public PaddleMovement Movement { get; private set; }
        
        [field: SerializeField]
        public PaddleCollision Collision { get; private set; }

        [field: SerializeField]
        public Transform BallOrigin { get; private set; }
    }
}
