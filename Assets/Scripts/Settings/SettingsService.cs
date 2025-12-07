using System;

public class SettingsService : ISettingsService
{
    private readonly ISettingsStorage _storage;

    public GameSettings Current { get; private set; }

    public event Action<GameplaySettings> GameplaySettingsChanged;

    public SettingsService(ISettingsStorage storage, GameSettings defaults)
    {
        _storage = storage;
        Current = defaults != null ? Clone(defaults) : new GameSettings();
        Load();
    }

    private GameSettings Clone(GameSettings source)
    {
        var json = UnityEngine.JsonUtility.ToJson(source);
        return UnityEngine.JsonUtility.FromJson<GameSettings>(json);
    }

    public void Apply(Action<GameSettings> mutate)
    {
        mutate?.Invoke(Current);
        GameplaySettingsChanged?.Invoke(Current.Gameplay);
    }

    public void Save()
    {
        _storage.Save(Current);
    }

    public void Load()
    {
        if (_storage.TryLoad(out var loaded))
        {
            Current = loaded;
        }
    }
}
