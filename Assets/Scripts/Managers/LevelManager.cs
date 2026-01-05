using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using VContainer.Unity;

public class LevelManager : IInitializable, IStartable, IDisposable
{
    public int Score { get; private set; } = 0;
    public bool IsInfinite { get; private set; } = false;

    public event UnityAction LevelEnded;
    public event UnityAction ShowPlayerInstructions;

    ITimerService _timerService;
    RecipeSystem _recipeSystem;
    LevelData _levelData;
    ILeaderboardService _leaderboardService;
    IGameStateService _gameStateService;



    public LevelManager(ITimerService timerService, RecipeSystem recipeSystem, SceneController sceneController, ILeaderboardService leaderboardService, IGameStateService gameStateService)
    {
        _timerService = timerService;
        _recipeSystem = recipeSystem;
        _levelData = sceneController.CurrentLevelData;
        _leaderboardService = leaderboardService;
        _gameStateService = gameStateService;
        IsInfinite = _levelData.isInfinite;
    }

    public void Initialize()
    {
        _gameStateService.SetGameState(GameState.Gameplay);

        _timerService.Completed += EndLevel;
        _recipeSystem.RecipeCompleted += CheckLevelType;
        _recipeSystem.AllRecipesCompleted += EndLevel;
    }

    public void Dispose()
    {
        _timerService.Completed -= EndLevel;
        _recipeSystem.RecipeCompleted -= CheckLevelType;
        _recipeSystem.AllRecipesCompleted -= EndLevel;
    }

    public void Start()
    {
        StartLevel();
    }

    public void StartLevel()
    {
        _timerService.Start(_levelData.levelDurationInSeconds);

        if (_levelData.showPlayerInstructions)
        {
            ShowPlayerInstructions?.Invoke();
        }
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
        if (_levelData.levelIndex > PlayerPrefs.GetInt("LevelCompleted"))
            PlayerPrefs.SetInt("LevelCompleted", _levelData.levelIndex);

        if (IsInfinite)
            _leaderboardService.SubmitScoreAsync("Player", Score);

        LevelEnded?.Invoke();
    }
}

