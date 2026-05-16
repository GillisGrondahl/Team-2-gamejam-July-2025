using UnityEngine;
using VContainer;

public class PlayerPrefsSettingsStorage : ISettingsStorage
{
    private const string Key = "game_settings_v1";

    [Inject]
    [UnityEngine.Scripting.Preserve]
    public PlayerPrefsSettingsStorage()
    {
        
    }

    public bool TryLoad(out GameSettings settings)
    {
        if (!PlayerPrefs.HasKey(Key))
        {
            settings = null;
            return false;
        }

        var json = PlayerPrefs.GetString(Key);
        if (string.IsNullOrEmpty(json))
        {
            settings = null;
            return false;
        }

        settings = JsonUtility.FromJson<GameSettings>(json);
        return settings != null;
    }

    public void Save(GameSettings settings)
    {
        var json = JsonUtility.ToJson(settings);
        PlayerPrefs.SetString(Key, json);
        PlayerPrefs.Save(); // important for WebGL
    }
}

