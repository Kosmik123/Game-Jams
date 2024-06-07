using Bipolar;
using Bipolar.Input;
using UnityEngine;

namespace NewHumanoid
{
    [System.Serializable]
    public class HumanoidMovement : HumanoidComponent
    {
        [SerializeField]
        private Serialized<IMoveInputProvider> inputProvider;

        public override void Update(CharacterController character)
        {
            
        }
    }
}
