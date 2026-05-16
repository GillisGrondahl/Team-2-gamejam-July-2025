using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;

public sealed class GameStateService : IGameStateService
{
    public GameState GameState { get; private set; } = GameState.Boot;
    public StateMask Baseline { get; private set; } = StateMask.None;
    public StateMask Overlay { get; private set; } = StateMask.None;
    public StateMask Effective { get; private set; } = StateMask.None;

    public event Action<GameState> GameStateChanged;
    public event Action<StateMask> MaskChanged;

    private readonly Dictionary<object, StateMask> _overlays = new();

    [Inject]
    [UnityEngine.Scripting.Preserve]
    public GameStateService()
    {

    }


    public void SetGameState(GameState state)
    {
        if (GameState == state) return;

        GameState = state;
        Baseline = GameModeDefaults.BaselineMask(GameState);
        GameStateChanged?.Invoke(GameState);
        Recompute(forceNotify: true);
    }

    public void Register(object owner, StateMask mask)
    {
        if (owner == null) throw new ArgumentNullException(nameof(owner));

        _overlays[owner] = mask;
        Recompute();
    }

    public void Unregister(object owner)
    {
        if (owner == null) return;
        if (_overlays.Remove(owner))
            Recompute();
    }

    public void ClearAll()
    {
        if (_overlays.Count == 0) return;
        _overlays.Clear();
        Recompute(forceNotify: true);
    }

    public bool IsRegistered(object owner)
        => owner != null && _overlays.ContainsKey(owner);

    private void Recompute(bool forceNotify = false)
    {
        //Debug.Log($"GameState: {GameState} | Entries: {_overlays.Count} | {string.Join(" ", _overlays.Select(kvp => $"{kvp.Key}={kvp.Value}"))}");
        StateMask overlay = StateMask.None;
        foreach (var kv in _overlays)
            overlay |= kv.Value;

        Overlay = overlay;

        var effective = Baseline | Overlay;
        if (!forceNotify && effective == Effective)
            return;

        Effective = effective;
        MaskChanged?.Invoke(Effective);
    }
}
