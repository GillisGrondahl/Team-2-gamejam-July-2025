using FMOD.Studio;
using FMODUnity;
using System.Collections.Generic;
using UnityEngine;


public class AudioManager : MonoBehaviour //, IAudioService
{
    private Bus masterBus;
    private Bus BGMBus;
    private Bus AMBBus;
    private Bus SFXBus;

    [Header("Volume Control")]
    [SerializeField, Range(0f, 1f)] private float _masterVolume = 0.9f;
    [SerializeField, Range(0f, 1f)] private float _bgmVolume = 0.7f;
    [SerializeField, Range(0f, 1f)] private float _ambVolume = 0.7f;
    [SerializeField, Range(0f, 1f)] private float _sfxVolume = 1.0f;

    public float MasterVolume
    {
        get => _masterVolume;
        set
        {
            _masterVolume = Mathf.Clamp01(value);
            SetVolume(masterBus, _masterVolume, nameof(masterBus));
        }
    }

    public float BgmVolume
    {
        get => _bgmVolume;
        set
        {
            _bgmVolume = Mathf.Clamp01(value);
            SetVolume(BGMBus, _bgmVolume, nameof(BGMBus));
        }
    }

    public float AmbVolume
    {
        get => _ambVolume;
        set
        {
            _ambVolume = Mathf.Clamp01(value);
            SetVolume(AMBBus, _ambVolume, nameof(AMBBus));
        }
    }

    public float SfxVolume
    {
        get => _sfxVolume;
        set
        {
            _sfxVolume = Mathf.Clamp01(value);
            SetVolume(SFXBus, _sfxVolume, nameof(SFXBus));
        }
    }

    [Header("BGM")]
    [SerializeField] private bool _bgmEnabled = true;

    public bool BGM_enabled
    {
        get => _bgmEnabled;
        set
        {
            _bgmEnabled = value;
            BGM_eventInstance.setPaused(_bgmEnabled);
        }
    }


    [SerializeField] private List<EventReference> BGM_trackEvents = new List<EventReference>();
    public string BGM_pitchName = "Pitch";
    [SerializeField][Range(0f, 1f)] private float BGM_originalPitchValue = 0f;
    [SerializeField][Range(0f, 1f)] private float BGM_pitchOnEarlyWarning = 0.3f;
    [SerializeField][Range(0f, 1f)] private float BGM_pitchOnFinalCountdown = 0.5f;

    private EventInstance BGM_eventInstance;
    private int currentBGMTrackIndex = -1; // Track which BGM is currently playing


    [Header("Ambience")]
    public EventReference AMB_trackEvent;
    private EventInstance AMB_EventInstance;

    private List<EventInstance> eventInstances; // list of FMOD event instances, we're keeping track so we can clean up (e.g., on a scene change)

    private void Awake()
    {
        eventInstances = new List<EventInstance>();
    }

    private void Start()
    {
        // Subscribe to game events
        //GameEvents.Instance.OnStartGameClicked += HandleLevelStart;
        //GameEvents.Instance.TimeManagerInstantiated += HandleTimeManagerInitiated;

        // Ensure banks are loaded (redundant if auto-loading works, but safe)
        if (!RuntimeManager.HasBankLoaded("Master"))
        {
            RuntimeManager.LoadBank("BGM");
            RuntimeManager.LoadBank("Ambience");
            RuntimeManager.LoadBank("SFX");
            RuntimeManager.LoadBank("Master");
        }

        // Wait for all banks to finish loading
        RuntimeManager.WaitForAllSampleLoading();

        // Assign busses
        masterBus = RuntimeManager.GetBus("bus:/");
        BGMBus = RuntimeManager.GetBus("bus:/BGM");
        AMBBus = RuntimeManager.GetBus("bus:/Ambience");
        SFXBus = RuntimeManager.GetBus("bus:/SFX");

        InitializeAmbience();

        //Load saved volume settings
        _masterVolume = PlayerPrefs.GetFloat(nameof(masterBus), _masterVolume);
        _bgmVolume = PlayerPrefs.GetFloat(nameof(BGMBus), _bgmVolume);
        _ambVolume = PlayerPrefs.GetFloat(nameof(AMBBus), _ambVolume);
        _sfxVolume = PlayerPrefs.GetFloat(nameof(SFXBus), _sfxVolume);

        // Set initial Volume
        SetVolume(masterBus, _masterVolume, nameof(masterBus));
        SetVolume(BGMBus, _bgmVolume, nameof(BGMBus));
        SetVolume(AMBBus, _ambVolume, nameof(AMBBus));
        SetVolume(SFXBus, _sfxVolume, nameof(SFXBus));
    }

    private void SetVolume(Bus volumeControlBus, float volume, string name)
    {
        volumeControlBus.setVolume(volume);
        PlayerPrefs.SetFloat(name, volume);

    }

    //public void HandleTimeManagerInitiated(TimeManager timeManager)
    //{
    //    //// Subscribe to TimeManager events
    //    //if (timeManager != null)   // only if we're in the main scene 
    //    //{
    //    //    timeManager.OnEarlyWarningReached += HandleEarlyWarningReached;
    //    //    timeManager.OnFinalCountdownReached += HandleFinalCountdownReached;
    //    //}
    //    ////SetTempo(BGM_originalPitchValue);
    //    //InitializeBGM();
    //}

    public void HandleLevelStart()
    {
        InitializeBGM();
        AMB_EventInstance.setParameterByName("WavesOnly", 0); // add the other ambience tracks when the level starts

    }

    public void HandleLevelStop()
    {
        BGM_eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        AMB_EventInstance.setParameterByName("WavesOnly", 1); // add the other ambience tracks when the level starts

    }

    public void InitializeBGM()
    {
        // Clean up existing BGM instance if it exists
        if (BGM_eventInstance.isValid())
        {
            BGM_eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            BGM_eventInstance.release();
            // Remove from tracking list if it exists there
            if (eventInstances.Contains(BGM_eventInstance))
            {
                eventInstances.Remove(BGM_eventInstance);
            }
        }


        if (BGM_trackEvents == null || BGM_trackEvents.Count == 0)
        {
            Debug.LogWarning("No BGM tracks are assigned.");
            return;
        }

        // Filter out null/empty event references
        List<EventReference> validTracks = new List<EventReference>();
        for (int i = 0; i < BGM_trackEvents.Count; i++)
        {
            if (!BGM_trackEvents[i].IsNull)
            {
                validTracks.Add(BGM_trackEvents[i]);
            }
        }

        if (validTracks.Count == 0)
        {
            Debug.LogWarning("No valid BGM tracks found.");
            return;
        }

        // Randomly select a track
        currentBGMTrackIndex = Random.Range(0, validTracks.Count);
        EventReference selectedTrack = validTracks[currentBGMTrackIndex];

        Debug.Log($"Playing BGM track {currentBGMTrackIndex + 1} of {validTracks.Count}");

        // Create and start the selected track
        BGM_eventInstance = RuntimeManager.CreateInstance(selectedTrack);
        eventInstances.Add(BGM_eventInstance);
        SetBGMTempo(BGM_originalPitchValue);
        BGM_eventInstance.start();

    }

    public void SetBGMTempo(float newPitch)
    {
        BGM_eventInstance.setParameterByName(BGM_pitchName, newPitch);
    }

    private void HandleEarlyWarningReached()
    {
        Debug.Log("Pitching up on early warning");
        SetBGMTempo(BGM_pitchOnEarlyWarning);
    }

    private void HandleFinalCountdownReached()
    {
        Debug.Log("Pitching up on final countdown");
        SetBGMTempo(BGM_pitchOnFinalCountdown);
    }

    public void InitializeAmbience()
    {
        if (AMB_trackEvent.IsNull)
        {
            Debug.LogWarning("Ambience track is not assigned.");
        }
        else
        {
            AMB_EventInstance = RuntimeManager.CreateInstance(AMB_trackEvent);
            eventInstances.Add(AMB_EventInstance);

            AMB_EventInstance.setParameterByName("WavesOnly", 1); // Waves only for the main menu
            AMB_EventInstance.start();
        }
    }

    public void PlayOneShot(EventReference sound, Vector3 worldPos)
    {
        RuntimeManager.PlayOneShot(sound, worldPos);
    }

    private void OnDestroy()
    {
        // Clean up FMOD instances
        foreach (EventInstance instance in eventInstances)
        {
            instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            instance.release();
        }
    }

    public void Initialize()
    {
        throw new System.NotImplementedException();
    }
}
