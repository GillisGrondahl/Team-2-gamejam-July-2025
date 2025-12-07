public interface IAudioService
{
    float MasterVolume { get; set; }
    float BgmVolume { get; set; }
    float AmbVolume { get; set; }
    float SfxVolume { get; set; }
    void SetTempo(TrackChannel channel, float newValue);
    void StartTrack(AudioTrackData track);
}
