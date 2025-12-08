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

    private IAudioService _audioService;
    private ISettingsService _settingsService;

    [Inject]
    private void Constructor(IAudioService audioService, ISettingsService settingsService)
    {
        _audioService = audioService;
        _settingsService = settingsService;
    }

    void OnEnable()
    {
        masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeSliderChanged);
        bgmVolumeSlider.onValueChanged.AddListener(OnBGMVolumeSliderChanged);
        ambienceVolumeSlider.onValueChanged.AddListener(OnAmbienceVolumeSliderChanged);
        sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeSliderChanged);

        landlubberMode.onValueChanged.AddListener(OnLandLubberModeChanged);
        oneArmedMode.onValueChanged.AddListener(OnOneHandModeChanged);

        exitButton.onClick.AddListener(OnExitButtonClicked);

        SetValues();
    }

    private void OnDisable()
    {
        masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeSliderChanged);
        bgmVolumeSlider.onValueChanged.RemoveListener(OnBGMVolumeSliderChanged);
        ambienceVolumeSlider.onValueChanged.RemoveListener(OnAmbienceVolumeSliderChanged);
        sfxVolumeSlider.onValueChanged.RemoveListener(OnSFXVolumeSliderChanged);

        landlubberMode.onValueChanged.RemoveListener(OnLandLubberModeChanged);
        oneArmedMode.onValueChanged.RemoveListener(OnOneHandModeChanged);

        exitButton.onClick.RemoveListener(OnExitButtonClicked);
    }

    private void SetValues()
    {
        masterVolumeSlider.value = _audioService.MasterVolume;
        bgmVolumeSlider.value = _audioService.BgmVolume;
        ambienceVolumeSlider.value = _audioService.AmbVolume;
        sfxVolumeSlider.value = _audioService.SfxVolume;

        landlubberMode.isOn = _settingsService.Current.Gameplay.LandlubberMode;
        oneArmedMode.isOn = _settingsService.Current.Gameplay.OneArmedMode;
    }

    private void OnExitButtonClicked()
    {
        _settingsService.Save();
        gameObject.SetActive(false);
    }

    public void OnLandLubberModeChanged(bool isOn) =>
        _settingsService.Apply(s => s.Gameplay.LandlubberMode = isOn);

    public void OnOneHandModeChanged(bool isOn) =>
        _settingsService.Apply(s => s.Gameplay.OneArmedMode = isOn);


    void OnMasterVolumeSliderChanged(float newVolume) => _audioService.MasterVolume = newVolume;
    void OnBGMVolumeSliderChanged(float newVolume) => _audioService.BgmVolume = newVolume;
    void OnAmbienceVolumeSliderChanged(float newVolume) => _audioService.AmbVolume = newVolume;
    void OnSFXVolumeSliderChanged(float newVolume) => _audioService.SfxVolume = newVolume;
}

