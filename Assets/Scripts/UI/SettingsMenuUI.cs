using System;
using UnityEngine;
using UnityEngine.UI;
using VContainer;


public class SettingsMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject settingsMenu;

    [SerializeField] private Button exitButton;

    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider bgmVolumeSlider;
    [SerializeField] private Slider ambienceVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    [SerializeField] private Toggle landlubberMode;
    [SerializeField] private Toggle oneArmedMode;

    AudioManager _audioManager;
    //LevelManager _levelManager;

    [Inject]
    private void Constructor(AudioManager audioManager)
    {
        _audioManager = audioManager;
        //_levelManager = levelManager;
    }

    void OnEnable()
    {
        masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeSliderChanged);
        bgmVolumeSlider.onValueChanged.AddListener(OnBGMVolumeSliderChanged);
        ambienceVolumeSlider.onValueChanged.AddListener(OnAmbienceVolumeSliderChanged);
        sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeSliderChanged);


        landlubberMode.onValueChanged.AddListener(OnLandLubberModeChanged);
        oneArmedMode.onValueChanged.AddListener(OnOneHandModeChanged);

        // set sliders to current values
        masterVolumeSlider.value = _audioManager.MasterVolume;
        bgmVolumeSlider.value = _audioManager.BgmVolume;
        ambienceVolumeSlider.value = _audioManager.AmbVolume;
        sfxVolumeSlider.value = _audioManager.SfxVolume;

    }

    private void OnDisable()
    {
        masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeSliderChanged);
        bgmVolumeSlider.onValueChanged.RemoveListener(OnBGMVolumeSliderChanged);
        ambienceVolumeSlider.onValueChanged.RemoveListener(OnAmbienceVolumeSliderChanged);
        sfxVolumeSlider.onValueChanged.RemoveListener(OnSFXVolumeSliderChanged);

        landlubberMode.onValueChanged.RemoveListener(OnLandLubberModeChanged);
        oneArmedMode.onValueChanged.RemoveListener(OnOneHandModeChanged);
    }

    private void OnEscapePressed(bool obj)
    {
        throw new NotImplementedException();
    }

    public void OnLandLubberModeChanged(bool isOn)
    {
        //LevelDifficultyManager.Instance.landlubberMode = landlubberMode.isOn;
        //GameEvents.Instance.OnLandlubberModeToggled?.Invoke(landlubberMode.isOn);
        Debug.Log("Landlubber mode toggled: " + landlubberMode.isOn);
    }

    public void OnOneHandModeChanged(bool isOn)
    {
        //GameEvents.Instance.OnOneArmedModeToggled?.Invoke(oneArmedMode.isOn);
        Debug.Log("One-armed mode toggled: " + oneArmedMode.isOn);

    }

    void OnMasterVolumeSliderChanged(float newVolume) => _audioManager.MasterVolume = newVolume;
    void OnBGMVolumeSliderChanged(float newVolume) => _audioManager.BgmVolume = newVolume;
    void OnAmbienceVolumeSliderChanged(float newVolume) => _audioManager.AmbVolume = newVolume;
    void OnSFXVolumeSliderChanged(float newVolume) => _audioManager.SfxVolume = newVolume;
}
