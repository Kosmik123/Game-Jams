using Bipolar;
using Bipolar.Input;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class CharacterComponent : MonoBehaviour
{
    protected CharacterController characterController;
    
    protected virtual void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }
}

public class PlayerMovement : CharacterComponent
{
    [SerializeField]
    private Serialized<IMoveInputProvider> inputProvider;
    public IMoveInputProvider InputProvider => inputProvider.Value;

    [SerializeField]
    private float moveSpeed = 4;

    [SerializeField]
    private Transform forwardProvider;

    private void FixedUpdate()
    {
        var direction = InputProvider.GetMotion();
        if (direction.sqrMagnitude > 1)
            direction.Normalize();

        var forwardProvider = this.forwardProvider ? this.forwardProvider : transform;
        var direction3D = forwardProvider.forward * direction.y + forwardProvider.right * direction.x;
        characterController.Move(moveSpeed * Time.fixedDeltaTime * direction3D);
    }
}
