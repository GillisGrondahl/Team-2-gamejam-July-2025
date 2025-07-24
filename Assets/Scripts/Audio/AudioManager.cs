using FMOD.Studio;
using FMODUnity;
using System.Collections.Generic;
using UnityEngine;


public class AudioManager : MonoBehaviour
{
    public static AudioManager instance { get; private set; }

    [Header("Volume Control")]
    [Range(0f, 1f)] public float masterVolume = 0.9f;
    [Range(0f, 1f)] public float BGM_volume = 0.7f;
    [Range(0f, 1f)] public float AMB_volume = 0.7f;
    [Range(0f, 1f)] public float SFX_volume = 1.0f;
    private Bus masterBus;
    private Bus BGMBus;
    private Bus AMBBus;
    private Bus SFXBus;

    [Header("BGM")]
    public bool BGM_enabled = true;
    [SerializeField] private List<EventReference> BGM_trackEvents = new List<EventReference>(); 
    public string BGM_pitchName = "Pitch";
    [SerializeField] [Range(0f, 1f)] private float BGM_originalPitchValue = 0f;    
    [SerializeField] [Range(0f, 1f)] private float BGM_pitchOnEarlyWarning = 0.3f;
    [SerializeField] [Range(0f, 1f)] private float BGM_pitchOnFinalCountdown = 0.5f;

    private EventInstance BGM_eventInstance;
    private int currentBGMTrackIndex = -1; // Track which BGM is currently playing


    [Header("Ambience")]
    public EventReference AMB_trackEvent;
    private EventInstance AMB_EventInstance;



    private List<EventInstance> eventInstances; // list of FMOD event instances, we're keeping track so we can clean up (e.g., on a scene change)



    private void Awake()
    {
        if (instance != null )
        {
            Debug.Log("More than one AudioManager in the scene!");  
        }
        instance = this;


        eventInstances = new List<EventInstance>();

    }

    private void Start()
    {
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

        InitializeBGM();
        InitializeAmbience();

        // Subscribe to TimeManager events
        if (TimeManager.Instance != null)   // only if we're in the main scene 
        {
            TimeManager.Instance.OnEarlyWarningReached += HandleEarlyWarningReached;
            TimeManager.Instance.OnFinalCountdownReached += HandleFinalCountdownReached;
        }

        // Subscribe to GameEvent events for audio settings
        GameEvents.Instance.OnMasterVolumeChanged += HandleMasterVolumeChanged;
        GameEvents.Instance.OnBGMVolumeChanged += HandleBGMVolumeChanged;
        GameEvents.Instance.OnAmbienceVolumeChanged += HandleAmbienceVolumeChanged;
        GameEvents.Instance.OnSFXVolumeChanged += HandleSFXVolumeChanged;

        // Set initial Volume
        SetVolume(masterBus, masterVolume);
        SetVolume(BGMBus, BGM_volume);
        SetVolume(AMBBus, AMB_volume);
        SetVolume(SFXBus, SFX_volume);
    }


    private void Update()
    {

        if (!BGM_enabled)
        {
            BGM_eventInstance.setPaused(true);
        }
        else
        {
            BGM_eventInstance.setPaused(false);
        }

    }

    #region VolumeControl
    private void SetVolume(Bus volumeControlBus, float volume)
    {
        volumeControlBus.setVolume(volume);
        
    }

    // Volume event handlers
    private void HandleMasterVolumeChanged(float newVolume)
    {
        masterVolume = newVolume;
        SetVolume(masterBus, masterVolume);
        Debug.Log($"Master volume changed to: {newVolume}");
    }

    private void HandleBGMVolumeChanged(float newVolume)
    {
        BGM_volume = newVolume;
        SetVolume(BGMBus, BGM_volume);
        Debug.Log($"BGM volume changed to: {newVolume}");
    }

    private void HandleAmbienceVolumeChanged(float newVolume)
    {
        AMB_volume = newVolume;
        SetVolume(AMBBus, AMB_volume);
        Debug.Log($"Ambience volume changed to: {newVolume}");
    }

    private void HandleSFXVolumeChanged(float newVolume)
    {
        SFX_volume = newVolume;
        SetVolume(SFXBus, SFX_volume);
        Debug.Log($"SFX volume changed to: {newVolume}");
    }



    #endregion

    #region BGM

    public void InitializeBGM()
    {
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
        SetTempo(BGM_originalPitchValue);
        BGM_eventInstance.start();

    }

    public void SetTempo(float newPitch)
    {
      

        BGM_eventInstance.setParameterByName(BGM_pitchName, newPitch);
        //BGM_eventInstance.getParameterByName(BGM_pitchName, out float currentPitch);
        //Debug.Log($"Pitch of BGM is now {currentPitch}");
    }

    private void HandleEarlyWarningReached()
    {
        Debug.Log("Pitching up on early warning");
        SetTempo(BGM_pitchOnEarlyWarning);
    }

    private void HandleFinalCountdownReached()
    {
        Debug.Log("Pitching up on final countdown");
        SetTempo(BGM_pitchOnFinalCountdown);
    }



    #endregion

    #region Ambience
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

            AMB_EventInstance.start();
        }
    }
    #endregion

    #region SFX
    public void PlayOneShot(EventReference sound, Vector3 worldPos)
    {
        RuntimeManager.PlayOneShot(sound, worldPos);
        
    }

    #endregion

    #region Cleanup
    private void OnDestroy()
    {
        // Clean up FMOD instances
        foreach (EventInstance instance in eventInstances)
        {
            instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            instance.release();
        }

        // Unsubscribe from events
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.OnMasterVolumeChanged -= HandleMasterVolumeChanged;
            GameEvents.Instance.OnBGMVolumeChanged -= HandleBGMVolumeChanged;
            GameEvents.Instance.OnAmbienceVolumeChanged -= HandleAmbienceVolumeChanged;
            GameEvents.Instance.OnSFXVolumeChanged -= HandleSFXVolumeChanged;
        }
    }

    #endregion
}
