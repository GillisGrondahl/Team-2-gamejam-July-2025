using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public sealed class GameStateApplier : IStartable, IDisposable
{
    IGameStateService _state;
    StateMask _currentMask;

    [Inject]
    public void Construct(IGameStateService state)
    {
        _state = state;
    }

    public void Start()
    {
        _state.MaskChanged += Apply;
    }

    public void Dispose()
    {
        _state.MaskChanged -= Apply;
    }
    private static void Apply(StateMask mask)
    {
        Time.timeScale = mask.HasFlag(StateMask.TimeScale) ? 0f : 1f;

        var visible = mask.HasFlag(StateMask.CursorVisible);
        var unlocked = mask.HasFlag(StateMask.CursorUnlocked);

        Cursor.lockState = unlocked ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = visible;
    }
}
