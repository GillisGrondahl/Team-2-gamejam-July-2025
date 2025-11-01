public interface IAudioService
{
    float MasterVolume { get; set; }
    float BgmVolume { get; set; }
    float AmbVolume { get; set; }
    float SfxVolume { get; set; }
    bool BGM_enabled { get; set; }

    void Initialize();
}
