using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using VContainer.Unity;

public class LevelManager : IInitializable, IStartable, IDisposable
{
    public event UnityAction GamePaused;
    public event UnityAction GameResumed;
    public event UnityAction LevelEnded;
    public event UnityAction ShowPlayerInstructions;

    private bool _isGamePaused = false;
    private bool _isLevelFinished = false;

    private IAudioService _audioService;
    private ITimerService _timerService;
    private IInputService _inputService;
    private RecipeSystem _recipeSystem;
    private LevelData _levelData;
    private ILeaderboardService _leaderboardService;

    public int Score { get; private set; } = 0;
    public bool IsInfinite { get; private set; } = false;

    public LevelManager(ITimerService timerService, IAudioService audioService, IInputService input, RecipeSystem recipeSystem, SceneController sceneController, ILeaderboardService leaderboardService)
    {
        _timerService = timerService;
        _audioService = audioService;
        _inputService = input;
        _recipeSystem = recipeSystem;
        _levelData = sceneController.CurrentLevelData;
        _leaderboardService = leaderboardService;
        IsInfinite = _levelData.isInfinite;
    }

    public void Start()
    {
        StartLevel();
    }

    public void StartLevel()
    {
        _isLevelFinished = false;
        _timerService.Start(_levelData.levelDurationInSeconds);

        if (_levelData.showPlayerInstructions)
        {
            ShowPlayerInstructions?.Invoke();
        }

        PauseGame();
    }

    private void HandleAudio()
    {
        if (_levelData.bgmTrack == null || _levelData.bgmTrack.Count == 0)
        {
            _audioService.StopTrack(TrackChannel.BGM);
        }
        else
        {
            _audioService.StartTrack(_levelData.bgmTrack[UnityEngine.Random.Range(0, _levelData.bgmTrack.Count)]);
        }

        if (_levelData.ambienceTrack == null || _levelData.ambienceTrack.Count == 0)
        {
            _audioService.StopTrack(TrackChannel.Ambience);
        }
        else
        {
            _audioService.StartTrack(_levelData.ambienceTrack[UnityEngine.Random.Range(0, _levelData.ambienceTrack.Count)]);
        }
    }

    public void Initialize()
    {
        _inputService.Escape += TogglePause;
        _timerService.Completed += EndLevel;
        _recipeSystem.RecipeCompleted += CheckLevelType;
        _recipeSystem.AllRecipesCompleted += EndLevel;
    }

    public void Dispose()
    {
        _inputService.Escape -= TogglePause;
        _timerService.Completed -= EndLevel;
        _recipeSystem.RecipeCompleted -= CheckLevelType;
        _recipeSystem.AllRecipesCompleted -= EndLevel;
    }

    public void CheckLevelType()
    {
        if (IsInfinite)
        {
            _timerService.AddTime(30);
            Score += _recipeSystem.CurrentRecipeScore;
            Debug.Log("Score: " + Score);
        }
    }

    private void EndLevel()
    {
        PauseGame();
        _isLevelFinished = true;
        if (_levelData.levelIndex > PlayerPrefs.GetInt("LevelCompleted"))
            PlayerPrefs.SetInt("LevelCompleted", _levelData.levelIndex);

        if (IsInfinite)
            _leaderboardService.SubmitScoreAsync("Player", Score);

        LevelEnded?.Invoke();
    }

    public void TogglePause()
    {
        if (_isLevelFinished) return;

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
        _timerService.Pause();
        _isGamePaused = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

    }

    private void ResumeGame()
    {
        Time.timeScale = 1f;
        _timerService.Resume();
        _isGamePaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}

