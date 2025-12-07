using UnityEngine;

[CreateAssetMenu(fileName = "AudioTrackData", menuName = "Data/AudioTrackData")]
public class AudioTrackData : ScriptableObject
{
    public TrackChannel Channel;
    public string ProviderKey;
}
