using System;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class LevelMenuUI : MonoBehaviour
{

    [SerializeField] private GameObject levelMenu;
    [SerializeField] private GameObject settingsMenu;
    [SerializeField] private GameObject readyButtonScreen;

    [SerializeField] private Button readyButton;

    [SerializeField] private Button restartButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button resumeButton;

    LevelManager _levelManager;
    ISceneController _sceneController;

    [Inject]
    private void Construct(LevelManager levelManager, ISceneController sceneController)
    {
        _levelManager = levelManager;
        _sceneController = sceneController;
    }

    private void OnEnable()
    {
        _levelManager.GamePaused += ShowLevelMenuUI;
        _levelManager.GameResumed += HideLevelMenuUI;
        _levelManager.ShowPlayerInstructions += OnShowPlayerInstucations;

        readyButton.onClick.AddListener(OnReadyButtonClicked);

        restartButton.onClick.AddListener(OnRestartButtonClicked);
        settingsButton.onClick.AddListener(OnSettingsButtonClicked);
        mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);
        resumeButton.onClick.AddListener(OnResumeButtonClicked);
    }

    private void OnDisable()
    {
        _levelManager.GamePaused -= ShowLevelMenuUI;
        _levelManager.GameResumed -= HideLevelMenuUI;
        _levelManager.ShowPlayerInstructions -= OnShowPlayerInstucations;

        readyButton.onClick.RemoveListener(OnReadyButtonClicked);

        restartButton.onClick.RemoveListener(OnRestartButtonClicked);
        settingsButton.onClick.RemoveListener(OnSettingsButtonClicked);
        mainMenuButton.onClick.RemoveListener(OnMainMenuButtonClicked);
        resumeButton.onClick.RemoveListener(OnResumeButtonClicked);
    }

    private void OnShowPlayerInstucations()
    {
        readyButtonScreen.gameObject.SetActive(false);
    }

    private void OnReadyButtonClicked()
    {
        readyButtonScreen.gameObject.SetActive(false);
        _levelManager.TogglePause();
    }

    private void OnRestartButtonClicked()
    {
        _sceneController.RetryCurrentLevel();
    }

    private void OnSettingsButtonClicked()
    {
        settingsMenu.SetActive(true);
    }

    private void OnMainMenuButtonClicked()
    {
        _sceneController.LoadMainMenu();
    }

    private void OnResumeButtonClicked()
    {
        _levelManager.TogglePause();
    }

    private void ShowLevelMenuUI()
    {
        levelMenu.SetActive(true);
    }

    private void HideLevelMenuUI()
    {
        levelMenu.SetActive(false);
    }
}
