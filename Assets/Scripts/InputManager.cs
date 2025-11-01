using System;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer.Unity;
using static InputSystem_Actions;

public class InputManager : IInputService, IStartable, IDisposable, IPlayerActions
{
    private InputSystem_Actions _actions;

    public Vector2 Direction => _actions.Player.Move.ReadValue<Vector2>();
    public bool IsInteractPressed => _actions.Player.Interact.IsPressed();
    public bool IsEscapePressed => _actions.Player.Escape.IsPressed();
    public bool IsOneArmedRMBPressed => _actions.Player.OneArmedRMB.IsPressed();

    public event Action<Vector2> Move = delegate { };
    public event Action<bool> Interact = delegate { };
    public event Action<float> Reach = delegate { };
    public event Action<Vector2> Look = delegate { };
    public event Action<Vector2> MousePosition = delegate { };
    public event Action<bool> OneArmedRMB = delegate { };
    public event Action<bool> Escape = delegate { };

    public void Start()
    {
        _actions = new InputSystem_Actions();
        _actions.Player.SetCallbacks(this);
        _actions.Enable();
    }

    public void Dispose()
    {
        _actions?.Disable();
        _actions?.Disable();
        _actions = null;
    }

    public void OnMove(InputAction.CallbackContext ctx) => Move(ctx.ReadValue<Vector2>());
    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (ctx.phase == InputActionPhase.Started) Interact(true);
        if (ctx.phase == InputActionPhase.Canceled) Interact(false);
    }
    public void OnReach(InputAction.CallbackContext ctx) => Reach(ctx.ReadValue<float>());
    public void OnLook(InputAction.CallbackContext ctx) => Look(ctx.ReadValue<Vector2>());
    public void OnMousePosition(InputAction.CallbackContext ctx) => MousePosition(ctx.ReadValue<Vector2>());
    public void OnOneArmedRMB(InputAction.CallbackContext ctx)
    {
        if (ctx.phase == InputActionPhase.Started) OneArmedRMB(true);
        if (ctx.phase == InputActionPhase.Canceled) OneArmedRMB(false);
    }

    public void OnEscape(InputAction.CallbackContext ctx)
    {
        if (ctx.phase == InputActionPhase.Started) Escape(true);
        if (ctx.phase == InputActionPhase.Canceled) Escape(false);
    }
}
