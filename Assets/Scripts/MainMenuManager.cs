using UnityEngine;
using VContainer;

public class MainMenuManager : MonoBehaviour
{
    AudioManager _audioManager;

    [Inject]
    private void Construct(AudioManager audioManager)
    {
        _audioManager = audioManager;
    }

    private void Start()
    {
        _audioManager.HandleLevelStop();
        //AudioManager.instance.HandleLevelStop();
    }
}
