using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static InputSystem_Actions;


[CreateAssetMenu(fileName = "InputReader", menuName = "ScriptableObjects/Input Reader")]
public class InputReader_SO : ScriptableObject, IPlayerActions /*, IInputService*/
{
    public event Action<Vector2> Move = delegate { };
    public event Action<bool> Interact = delegate { };
    public event Action<float> Reach = delegate { };
    public event Action<Vector2> Look = delegate { };
    public event Action<Vector2> MousePosition = delegate { };
    public event Action<bool> OneArmedRMB = delegate { };

    public InputSystem_Actions _actions;

    public Vector2 Direction => _actions.Player.Move.ReadValue<Vector2>();
    public bool IsInteractPressed => _actions.Player.Interact.IsPressed();
    public bool IsOneArmedRMBPressed => _actions.Player.OneArmedRMB.IsPressed();

    public void EnableInputActions()
    {
        if(_actions == null)
        {
            _actions = new InputSystem_Actions();
            _actions.Player.SetCallbacks(this);
        }
        _actions.Enable();
    }

    public void OnOneArmedRMB(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Started:
                OneArmedRMB.Invoke(true);
                break;
            case InputActionPhase.Canceled:
                OneArmedRMB.Invoke(false);
                break;
        }
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

    public void OnEscape(InputAction.CallbackContext context)
    {
        // Implement escape functionality if needed
    }
}
