public interface IAudioService
{
    float MasterVolume { get; set; }
    float BgmVolume { get; set; }
    float AmbVolume { get; set; }
    float SfxVolume { get; set; }
    void SetParameter(TrackChannel channel, string paramName, float newValue);
    void StartTrack(AudioTrackData track, float fadeOutSeconds = 0.5f, float fadeInSeconds = 0.5f);
    void StopTrack(TrackChannel channel, float fadeOutSeconds = 0f);
    void ResetAudio();
    
}
