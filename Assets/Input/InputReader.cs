using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static InputSystem_Actions;


public interface IInputReader
{
    Vector2 Direction { get; }
    void EnableInputActions(); 
}

[CreateAssetMenu(fileName = "InputReader", menuName = "ScriptableObjects/Input Reader")]
public class InputReader : ScriptableObject, IPlayerActions, IInputReader
{
    public event UnityAction<Vector2> Move = delegate { };
    public event UnityAction<bool> Interact = delegate { };
    public event UnityAction<float> Reach = delegate { };
    public event UnityAction<Vector2> Look = delegate { };
    public event UnityAction<Vector2> MousePosition = delegate { };

    public InputSystem_Actions inputActions;

    public Vector2 Direction => inputActions.Player.Move.ReadValue<Vector2>();
    public bool IsInteractKeyPressed => inputActions.Player.Interact.IsPressed();

    public void EnableInputActions()
    {
        if(inputActions == null)
        {
            inputActions = new InputSystem_Actions();
            inputActions.Player.SetCallbacks(this);
        }
        inputActions.Enable();
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Started:
                Interact.Invoke(true);
                break;
            case InputActionPhase.Canceled:
                Interact.Invoke(false);
                break;
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Move.Invoke(context.ReadValue<Vector2>());
    }

    public void OnReach(InputAction.CallbackContext context)
    {
        Reach.Invoke(context.ReadValue<float>());
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        Look.Invoke(context.ReadValue<Vector2>());
    }

    public void OnMousePosition(InputAction.CallbackContext context)
    {
        MousePosition.Invoke(context.ReadValue<Vector2>());
    }
}
