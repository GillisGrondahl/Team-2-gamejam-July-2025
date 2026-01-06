using System;


public enum GameState
{
    Boot,
    MainMenu,
    LevelSelect,
    Gameplay
}

[Flags]
public enum StateMask
{
    None = 0,
    TimeScale = 1 << 0, // sets Time.timeScale = 0
    GameplayInput = 1 << 1, // disable gameplay actions
    UIInput = 1 << 2, // enable UI actions (optional)
    CursorVisible = 1 << 3, // show cursor
    CursorUnlocked = 1 << 4, // unlock cursor
    AudioDuck = 1 << 5, // snapshot / mixer duck (optional)
}

public static class GameModeDefaults
{
    public static StateMask BaselineMask(GameState mode) => mode switch
    {
        GameState.MainMenu or GameState.LevelSelect =>
            StateMask.UIInput | StateMask.CursorVisible | StateMask.CursorUnlocked | StateMask.GameplayInput,

        GameState.Gameplay =>
            StateMask.None, // cursor hidden/locked handled by baseline or by a separate "GameplayLook" policy if desired

        _ => StateMask.None
    };
}

public interface IGameStateService
{
    GameState GameState { get; }
    StateMask Baseline {  get; }
    StateMask Overlay { get; }
    StateMask Effective { get; }

    event Action<GameState> GameStateChanged;
    event Action<StateMask> MaskChanged;

    void SetGameState(GameState state);
    void Register(object owner, StateMask mask);
    void Unregister(object owner);
    void ClearAll();

    bool IsRegistered(object owner);
}

