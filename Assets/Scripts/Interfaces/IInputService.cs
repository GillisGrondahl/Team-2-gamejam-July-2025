using System;
using UnityEngine;

public interface IInputService
{
    Vector2 Direction { get; }
    bool IsInteractPressed { get; }
    bool IsEscapePressed { get; }
    bool IsOneArmedRMBPressed { get; }

    event Action<Vector2> Move;
    event Action<bool> Interact;
    event Action<float> Reach;
    event Action<Vector2> Look;
    event Action<Vector2> MousePosition;
    event Action<bool> OneArmedRMB;
    event Action Escape;
}
