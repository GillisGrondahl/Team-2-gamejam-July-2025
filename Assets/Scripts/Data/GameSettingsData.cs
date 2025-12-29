using UnityEngine;

[CreateAssetMenu(fileName = "GameSettingsDefaults", menuName = "Config/Game Settings Defaults")]
public class GameSettingsData : ScriptableObject
{
    public GameSettings Value = new GameSettings();
}

