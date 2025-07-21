using UnityEngine;
using UnityEngine.UI;


public class SettingsMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject settingsMenu;

    [SerializeField] private Button exitButton;

    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider bgmVolumeSlider;
    [SerializeField] private Slider ambienceVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;


    void Start()
    {
        GameEvents.Instance.OnSettingsClicked += HandleSettingsClicked;

        exitButton.onClick.AddListener(() =>
        {
            CloseSettingsMenu();
        });

        masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeSliderChanged);
        bgmVolumeSlider.onValueChanged.AddListener(OnBGMVolumeSliderChanged);
        ambienceVolumeSlider.onValueChanged.AddListener(OnAmbienceVolumeSliderChanged);
        sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeSliderChanged);


        // set sliders to current values
        masterVolumeSlider.value = AudioManager.instance.masterVolume;
        bgmVolumeSlider.value = AudioManager.instance.BGM_volume;
        ambienceVolumeSlider.value = AudioManager.instance.AMB_volume;
        sfxVolumeSlider.value = AudioManager.instance.AMB_volume;

    }

    
    void HandleSettingsClicked()
    {
        settingsMenu.SetActive(true);
    }


    void CloseSettingsMenu()
    {
        settingsMenu.SetActive(false);
    }

    void OnMasterVolumeSliderChanged(float newVolume)
    {
        GameEvents.Instance.OnMasterVolumeChanged?.Invoke(newVolume);
    }

    void OnBGMVolumeSliderChanged(float newVolume)
    {
        GameEvents.Instance.OnBGMVolumeChanged?.Invoke(newVolume);
    }

    void OnAmbienceVolumeSliderChanged(float newVolume)
    {
        GameEvents.Instance.OnAmbienceVolumeChanged?.Invoke(newVolume);
    }

    void OnSFXVolumeSliderChanged(float newVolume)
    {
        GameEvents.Instance.OnSFXVolumeChanged?.Invoke(newVolume);
    }
}
