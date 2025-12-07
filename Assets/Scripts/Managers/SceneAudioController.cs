using UnityEngine;
using VContainer;

public class SceneAudioController : MonoBehaviour
{
    [SerializeField] private AudioTrackData _bgmTrack;
    [SerializeField] private AudioTrackData _ambienceTrack;

    private IAudioService _audioService;

    [Inject]
    public void Construct(IAudioService audioService)
    {
        _audioService = audioService;
    }

    private void Start()
    {
        _audioService.StartTrack(_bgmTrack);
        _audioService.StartTrack(_ambienceTrack);
    }



}

