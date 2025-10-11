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

    [SerializeField] private Toggle landlubberMode;
    [SerializeField] private Toggle oneArmedMode;


    void OnEnable()
    {
        //GameEvents.Instance.OnSettingsClicked += HandleSettingsClicked;

        /*
        exitButton.onClick.AddListener(() =>
        {
            CloseSettingsMenu();
        });

        */

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

    private void OnDisable()
    {
        // GameEvents.Instance.OnSettingsClicked -= HandleSettingsClicked;
    }

    
    public void OnLandlubberModeToggled()
    {
        LevelDifficultyManager.Instance.landlubberMode = landlubberMode.isOn;
        GameEvents.Instance.OnLandlubberModeToggled?.Invoke(landlubberMode.isOn);
        Debug.Log("Landlubber mode toggled: " + landlubberMode.isOn);
    }

    public void OnOneHandedModeToggled(bool isOn)
    {
        GameEvents.Instance.OnOneArmedModeToggled?.Invoke(oneArmedMode.isOn);
        Debug.Log("One-armed mode toggled: " + oneArmedMode.isOn);

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
