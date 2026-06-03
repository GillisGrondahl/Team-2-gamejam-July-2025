using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class LevelSelectionManager : MonoBehaviour
{
    [SerializeField] private List<StepsActivator> levelStepsActivators = new();

    [SerializeField] private int levelCompleted = 0;

    SceneController _sceneController;

    IGameStateService _state;

    StateMask LevelSelectionMask = StateMask.CursorVisible | StateMask.CursorUnlocked;

    [Inject]
    public void Constructor(SceneController sceneController, IGameStateService stateService)
    {
        _sceneController = sceneController;
        _state = stateService;
    }
    private void Awake()
    {
        _state.SetGameState(GameState.LevelSelect);
        levelCompleted = PlayerPrefs.GetInt("LevelCompleted");
    }

    private void Start()
    {
        UpdateLevelSteps();
    }

    private void UpdateLevelSteps()
    {
        for (int i = 0; i < levelStepsActivators.Count; i++)
        {
            if (i < levelCompleted)
            {
                levelStepsActivators[i].ShowStepsInstantly();
            }
            else if (levelCompleted == i)
            {
                levelStepsActivators[i].ShowStepsSequentially();
            }
            else
            {
                levelStepsActivators[i].HideSteps();
            }
        }
    }

    public void LoadLevel(LevelData levelData)
    {
        _sceneController.LoadSceneByLevelData(levelData);
    }

    public void LoadMainMenu()
    {
        _sceneController.LoadMainMenu();
    }
}
