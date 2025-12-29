using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class SceneAudioController : MonoBehaviour
{
    [SerializeField] private List<AudioTrackData> _bgmTracks;
    [SerializeField] private List<AudioTrackData> _ambienceTracks;

    private IAudioService _audioService;

    [Inject]
    public void Construct(IAudioService audioService)
    {
        _audioService = audioService;
    }

    private void Start()
    {
        if (_bgmTracks == null || _bgmTracks.Count == 0)
        {
            _audioService.StopTrack(TrackChannel.BGM);
        }
        else
        {
            _audioService.StartTrack(_bgmTracks[UnityEngine.Random.Range(0, _bgmTracks.Count)]);
        }

        if (_ambienceTracks == null || _ambienceTracks.Count == 0)
        {
            _audioService.StopTrack(TrackChannel.Ambience);
        }
        else
        {
            _audioService.StartTrack(_ambienceTracks[UnityEngine.Random.Range(0, _ambienceTracks.Count)]);
        }

    }



}

