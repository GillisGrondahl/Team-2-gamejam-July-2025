using FMOD.Studio;
using FMODUnity;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance { get; private set; }

    [Header("BGM")]
    public EventReference BGM_trackEvent;
    public string BGM_pitchName = "Pitch";
    [Range(0f, 1f)] public float BGM_pitchValue = 0f;
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
            RuntimeManager.LoadBank("Master");
        }

        // Wait for all banks to finish loading
        RuntimeManager.WaitForAllSampleLoading();


        InitializeBGM();
        InitializeAmbience();
    }


    private void Update()
    {
        SetTempo(BGM_pitchValue);
    }

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
            BGM_eventInstance.start();
        }
        
    }

    public void SetTempo(float pitch)
    {
      

        BGM_eventInstance.setParameterByName(BGM_pitchName, pitch);
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
