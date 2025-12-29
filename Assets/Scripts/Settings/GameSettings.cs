using System;

[Serializable]
public class GameSettings
{
    public AudioSettings Audio = new();
    public VideoSettings Video = new();
    public GameplaySettings Gameplay = new();
}

[Serializable]
public class AudioSettings
{
    public float MasterVolume = 1f;
    public float MusicVolume = 0.8f;
    public float AmbienceVolume = 0.8f;
    public float SfxVolume = 0.8f;
}

[Serializable]
public class VideoSettings
{
    public int ResolutionIndex = 0;
    public bool Fullscreen = true;
    public int QualityLevel = 2;
}

[Serializable]
public class GameplaySettings
{
    public float MouseSensitivity = 1f;
    public bool InvertY = false;
    public bool LandlubberMode = false;
    public bool OneArmedMode = false;
}

