using UnityEngine;

namespace NewHumanoid
{
    [System.Serializable]
    public class JumpComponent : HumanoidComponent
    {
        [SerializeField]
        private float jumpForce;
        [SerializeField]
        private float coyoteTime;

        public override void Update(CharacterController character)
        {
            throw new System.NotImplementedException();
        }
    }
}
