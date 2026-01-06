public interface IAudioService
{
    float MasterVolume { get; set; }
    float BgmVolume { get; set; }
    float AmbVolume { get; set; }
    float SfxVolume { get; set; }
    void SetParameter(TrackChannel channel, string paramName, float newValue);
    void StartTrack(AudioTrackData track, bool fadeOut = true, float fadeInSeconds = 0.5f);
    void StopTrack(TrackChannel channel, bool fadeOut = true);
    void ResetAudio();
    
}
