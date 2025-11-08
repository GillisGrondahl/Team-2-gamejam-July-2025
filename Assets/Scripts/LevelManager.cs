using FMOD;
using System;
using UnityEngine;
using UnityEngine.Events;
using VContainer;
using VContainer.Unity;

public class LevelManager : IInitializable, IStartable, IDisposable
{
    private bool _isGamePaused = false;
    private bool _isLevelFinished = false;

    private AudioManager _audio;
    private ITimerService _timer;
    private IInputService _input;
    private RecipeSystem _recipeSystem;
    private LevelData _levelData;

    public event UnityAction GamePaused;
    public event UnityAction GameResumed;
    public event UnityAction LevelEnded;

    public event UnityAction ShowPlayerInstructions;

    public LevelManager(ITimerService timerService, AudioManager audioService, IInputService input, RecipeSystem recipeSystem, LevelData levelData)
    {
        _timer = timerService;
        _audio = audioService;
        _input = input;
        _recipeSystem = recipeSystem;
        _levelData = levelData;
    }

    public void Start()
    {
        StartLevel();
    }

    public void StartLevel()
    {
        _isLevelFinished = false;
        _timer.Start(_levelData.levelDurationInSeconds);
        _audio.HandleLevelStart();

        if(_levelData.showPlayerInstructions)
        {
            ShowPlayerInstructions?.Invoke();
            PauseGame();
        }
        else
            ResumeGame();
    }

    public void Initialize()
    {
        _input.Escape += TogglePause;
        _timer.Completed += OnLevelEnd;
        _recipeSystem.AllRecipesCompleted += OnLevelEnd;
    }

    public void Dispose()
    {
        _input.Escape -= TogglePause;
        _timer.Completed -= OnLevelEnd;
        _recipeSystem.AllRecipesCompleted -= OnLevelEnd;
    }

    private void OnLevelEnd()
    {
        PauseGame();
        _isLevelFinished = true;
        LevelEnded?.Invoke();
    }

    public void TogglePause()
    {
        if(_isLevelFinished) return;

        if (_isGamePaused)
        {
            ResumeGame();
            GameResumed?.Invoke();
        }
        else
        {
            PauseGame();
            GamePaused?.Invoke();
        }
    }
    private void PauseGame()
    {
        Time.timeScale = 0f;
        _timer.Pause();
        _isGamePaused = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

    }

    private void ResumeGame()
    {
        Time.timeScale = 1f;
        _timer.Resume();
        _isGamePaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}

