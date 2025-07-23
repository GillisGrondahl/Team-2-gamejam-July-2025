using System;
using UnityEngine;

public class GameEvents : MonoBehaviour
{
    public static GameEvents Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // Game State Events
    public System.Action OnStartGameClicked;
    public System.Action OnExitGameClicked;
    public System.Action OnSettingsClicked;

    // Settings Events
    public System.Action<float> OnMasterVolumeChanged;
    public System.Action<float> OnBGMVolumeChanged;
    public System.Action<float> OnAmbienceVolumeChanged;
    public System.Action<float> OnSFXVolumeChanged;



    private void Start()
    {
        
        OnStartGameClicked += HandleStartGameClicked;
        OnExitGameClicked += HandleExitGameClicked;
        OnSettingsClicked += HandleSettingsClicked;
     }



    public void HandleStartGameClicked()
    {
        Debug.Log("Start Game Clicked");
    }

    public void HandleSettingsClicked()
    {
        Debug.Log("Settings Clicked");
    }

    public void HandleExitGameClicked()
    {
        Debug.Log("Exit Game Clicked");
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif

    }
}


