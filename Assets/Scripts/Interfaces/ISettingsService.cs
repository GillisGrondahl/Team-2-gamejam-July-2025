using System;

public interface ISettingsService
{
    GameSettings Current { get; }
    void Apply(Action<GameSettings> mutate);
    void Save();
    void Load();

    event Action<GameplaySettings> GameplaySettingsChanged;
}

