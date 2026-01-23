using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class SceneAudioController : MonoBehaviour
{
    [SerializeField] private List<AudioTrackData> _bgmTracks;
    [SerializeField] private List<AudioTrackData> _ambienceTracks;

    private IAudioService _audioService;
    private LevelData _levelData;

    [Inject]
    public void Construct(IAudioService audioService, SceneController sceneController)
    {
        _audioService = audioService;
        _levelData = sceneController.CurrentLevelData;
    }

    private void Start()
    {
        _audioService.ResetAudio();

        List<AudioTrackData> bgmTracks = _levelData ? _levelData.bgmTracks : _bgmTracks;
        List<AudioTrackData> ambienceTracks = _levelData ? _levelData.ambienceTracks : _ambienceTracks;


        if (bgmTracks == null || bgmTracks.Count == 0)
        {
            _audioService.StopTrack(TrackChannel.BGM, true);
        }
        else
        {
            _audioService.StartTrack(bgmTracks[Random.Range(0, bgmTracks.Count)], true);
        }

        if (ambienceTracks == null || ambienceTracks.Count == 0)
        {
            _audioService.StopTrack(TrackChannel.Ambience, true);
        }
        else
        {
            _audioService.StartTrack(ambienceTracks[Random.Range(0, ambienceTracks.Count)], true, 1f);
        }

    }



}

