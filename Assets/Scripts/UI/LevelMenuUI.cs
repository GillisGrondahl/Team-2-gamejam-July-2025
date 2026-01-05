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
    IInputService _inputService;
    IGameStateService _state;

    private static readonly StateMask MenuMask =
        StateMask.TimeScale |
        StateMask.CursorVisible |
        StateMask.CursorUnlocked;

    [Inject]
    private void Construct(LevelManager levelManager, ISceneController sceneController, IGameStateService stateService, IInputService inputService)
    {
        _levelManager = levelManager;
        _sceneController = sceneController;
        _inputService = inputService;
        _state = stateService;

    }

    private void Awake()
    {
        _state.Register(readyButton, MenuMask);
    }


    private void OnEnable()
    {
        //_levelManager.GamePaused += ShowLevelMenuUI;
        //_levelManager.GameResumed += HideLevelMenuUI;
        _inputService.Escape += ToggleMenu;
        _levelManager.ShowPlayerInstructions += OnShowPlayerInstucations;

        readyButton.onClick.AddListener(OnReadyButtonClicked);

        restartButton.onClick.AddListener(OnRestartButtonClicked);
        settingsButton.onClick.AddListener(OnSettingsButtonClicked);
        mainMenuButton.onClick.AddListener(OnLevelSelectionButtonClicked);
        resumeButton.onClick.AddListener(OnResumeButtonClicked);
    }

    private void OnDisable()
    {
        //_levelManager.GamePaused -= ShowLevelMenuUI;
        //_levelManager.GameResumed -= HideLevelMenuUI;
        _inputService.Escape -= ToggleMenu;
        _levelManager.ShowPlayerInstructions -= OnShowPlayerInstucations;

        readyButton.onClick.RemoveListener(OnReadyButtonClicked);

        restartButton.onClick.RemoveListener(OnRestartButtonClicked);
        settingsButton.onClick.RemoveListener(OnSettingsButtonClicked);
        mainMenuButton.onClick.RemoveListener(OnLevelSelectionButtonClicked);
        resumeButton.onClick.RemoveListener(OnResumeButtonClicked);
    }

    private void ToggleMenu()
    {
        if (!levelMenu.activeSelf)
        {
            levelMenu.SetActive(true);
            _state.Register(this, MenuMask);
        }
        else
        {
            levelMenu.SetActive(false);
            _state.Unregister(this);
        }
    }

    private void OnShowPlayerInstucations()
    {
        readyButtonScreen.gameObject.SetActive(false);
        _state.Unregister(readyButton);
    }

    private void OnReadyButtonClicked()
    {
        readyButtonScreen.gameObject.SetActive(false);
        _state.Unregister(readyButton);

    }

    private void OnRestartButtonClicked()
    {
        _sceneController.RetryCurrentLevel();
    }

    private void OnSettingsButtonClicked()
    {
        settingsMenu.SetActive(true);
    }

    private void OnLevelSelectionButtonClicked()
    {
        _sceneController.LoadLevelSelection();
    }

    private void OnResumeButtonClicked()
    {
        ToggleMenu();
    }

    private void OnDestroy()
    {
        _state.Unregister(readyButton);
        _state.Unregister(this);
    }
}
