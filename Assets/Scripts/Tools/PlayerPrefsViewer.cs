using UnityEngine;

public class PlayerPrefsViewer : MonoBehaviour
{
    public string[] keysToInspect = { "leaderboard_v1", "game_settings_v1" };

    void Start()
    {
        foreach (var key in keysToInspect)
        {
            if (PlayerPrefs.HasKey(key))
                Debug.Log($"{key}:\n{PlayerPrefs.GetString(key)}");
            else
                Debug.Log($"{key}: <not found>");
        }
    }
}