using FMOD.Studio;
using FMODUnity;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance { get; private set; }

    [Header("Volume Control")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float BGM_volume = 1f;
    [Range(0f, 1f)] public float AMB_volume = 1f;
    [Range(0f, 1f)] public float SFX_volume = 1f;
    private Bus masterBus;
    private Bus BGMBus;
    private Bus AMBBus;
    private Bus SFXBus;

    [Header("BGM")]
    public bool BGM_enabled = true;
    public EventReference BGM_trackEvent;
    public string BGM_pitchName = "Pitch";
    [SerializeField] [Range(0f, 1f)] private float BGM_originalPitchValue = 0f;    
    [SerializeField] [Range(0f, 1f)] private float BGM_pitchOnEarlyWarning = 0.3f;
    [SerializeField] [Range(0f, 1f)] private float BGM_pitchOnFinalCountdown = 0.5f;

    private EventInstance BGM_eventInstance;



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
        TimeManager.Instance.OnEarlyWarningReached += HandleEarlyWarningReached;
        TimeManager.Instance.OnFinalCountdownReached += HandleFinalCountdownReached;
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


        // Basic volume control (events subscription to be implemented later)
        masterBus.getVolume(out float currentVolume);
        if (masterVolume != currentVolume)
        {
            SetVolume(masterBus, masterVolume);
        }
        BGMBus.getVolume(out currentVolume);
        if (BGM_volume != currentVolume)
        {
            SetVolume(BGMBus, BGM_volume);
        }
        AMBBus.getVolume(out currentVolume);
        if (AMB_volume != currentVolume)
        {
            SetVolume(AMBBus, AMB_volume);
        }
        SFXBus.getVolume(out currentVolume);
        if (SFX_volume != currentVolume)
        {
            SetVolume(SFXBus, SFX_volume);
        }


    }

    #region VolumeControl
    private void SetVolume(Bus volumeControlBus, float volume)
    {
        volumeControlBus.setVolume(volume);
        
    }

    



    #endregion

    #region BGM

    public void InitializeBGM()
    {
        if (BGM_trackEvent.IsNull)
        {
            Debug.LogWarning("BGM track is not assigned.");
        }
        else
        {
            BGM_eventInstance = RuntimeManager.CreateInstance(BGM_trackEvent);
            eventInstances.Add(BGM_eventInstance);
            SetTempo(BGM_originalPitchValue);
            BGM_eventInstance.start();
        }
        
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
    private void CleanUp()
    {
        foreach (EventInstance instance in eventInstances)
        {
            instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            instance.release();
        }

    }

    private void OnDestroy()
    {
        CleanUp();
    }

    #endregion
}
