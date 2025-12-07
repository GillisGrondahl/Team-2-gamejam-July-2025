using System.IO;
using UnityEngine;

public class FileSettingsStorage : ISettingsStorage
{
    private readonly string _path;

    public FileSettingsStorage()
    {
        _path = Path.Combine(Application.persistentDataPath, "game_settings_v1.json");
    }

    public bool TryLoad(out GameSettings settings)
    {
        if (!File.Exists(_path))
        {
            settings = null;
            return false;
        }

        var json = File.ReadAllText(_path);
        settings = JsonUtility.FromJson<GameSettings>(json);
        return settings != null;
    }

    public void Save(GameSettings settings)
    {
        var json = JsonUtility.ToJson(settings, true);
        File.WriteAllText(_path, json);
    }
}

