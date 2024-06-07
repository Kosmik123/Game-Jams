using UnityEngine;

namespace NewHumanoid
{
    [System.Serializable]
    public class SprintComponent : HumanoidComponent
    {
        [SerializeField]
        private KeyCode sprintKey = KeyCode.LeftShift;
        [SerializeField]
        private float speedBoost;

        public override void Update(CharacterController character)
        {
            throw new System.NotImplementedException();
        }
    }
}
