using Bipolar;
using Bipolar.Input;
using UnityEngine;

public class CharacterJump : CharacterComponent
{
    [SerializeField]
    private Serialized<IActionInputProvider> inputProvider;

    private void OnEnable()
    {
        inputProvider.Value.OnPerformed += Value_OnPerformed;
    }

    private void Value_OnPerformed()
    {

    }

    private void OnDisable()
    {
        inputProvider.Value.OnPerformed -= Value_OnPerformed;
    }
}

public class CharacterGravity : CharacterComponent
{
    private void Update()
    {
        
    }
}