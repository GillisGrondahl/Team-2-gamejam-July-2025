using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance { get; private set; }

    [Header("BGM")]
    public EventReference BGM_trackEvent;
    public string BGM_pitchName = "Pitch";
    [Range(0f, 1f)] public float BGM_pitchValue = 0f;
    private EventInstance BGM_musicEventInstance;



    [Header("Ambience")]
    public EventReference AMB_trackEvent;
    private EventInstance AMB_EventInstance;




    private void Awake()
    {
        if (instance != null )
        {
            Debug.Log("More than one AudioManager in the scene!");  
        }
        instance = this;
        
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
            BGM_musicEventInstance = RuntimeManager.CreateInstance(BGM_trackEvent);
            BGM_musicEventInstance.start();
        }
        
    }

    public void SetTempo(float pitch)
    {
      

        BGM_musicEventInstance.setParameterByName(BGM_pitchName, pitch);
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

}
