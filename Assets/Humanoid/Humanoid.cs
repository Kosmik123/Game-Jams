using UnityEngine;

namespace NewHumanoid
{
    [RequireComponent(typeof(CharacterController))]
    public class Humanoid : MonoBehaviour
    {
        private CharacterController characterController;

        [SerializeReference, SubclassSelector]
        private HumanoidComponent[] components;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
        }

        private void Update()
        {
            foreach (var component in components)
                component?.Update(characterController);
        }

        protected virtual void OnValidate()
        {
            foreach (var component in components)
                component?.Validate();
        }

    }

    [System.Serializable]
    public abstract class HumanoidComponent
    {
        [HideInInspector, SerializeField]
        private string name;

        [SerializeField]
        private bool enabled;
        public bool Enabled
        {
            get => enabled;
            set
            {
                enabled = value;
            }
        }

        public abstract void Update(CharacterController character);

        public void Validate()
        {
            name = GetType().Name.Replace("Component", string.Empty);
#if UNITY_EDITOR
            name = UnityEditor.ObjectNames.NicifyVariableName(name);
#endif
            OnValidate();
        }

        protected virtual void OnValidate() { }
    }
}
