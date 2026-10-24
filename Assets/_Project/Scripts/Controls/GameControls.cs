using UnityEngine;
using UnityEngine.InputSystem;

namespace ZeroHour.Controls
{
    // Shared input for keyboard, gamepad and the on-screen mobile joystick (which drives a virtual gamepad).
    public static class GameControls
    {
        private static InputAction moveAction;

        public static InputAction MoveAction => moveAction ??= CreateMoveAction();

        public static Vector2 Move => MoveAction.ReadValue<Vector2>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            moveAction?.Dispose();
            moveAction = null;
        }

        private static InputAction CreateMoveAction()
        {
            var action = new InputAction("Move", InputActionType.Value, expectedControlType: "Vector2");
            action.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            action.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/rightArrow");
            action.AddBinding("<Gamepad>/leftStick");
            action.Enable();
            return action;
        }
    }
}
