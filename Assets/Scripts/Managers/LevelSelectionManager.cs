using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class LevelSelectionManager : MonoBehaviour
{
    [SerializeField] private List<StepsActivator> levelStepsActivators = new();

    [SerializeField] private int levelCompleted = 0;

    SceneController _sceneController;

    [Inject]
    private void Constructor(SceneController sceneController)
    {
        _sceneController = sceneController;
    }

    private void Start()
    {
        Time.timeScale = 1f;
        levelCompleted = PlayerPrefs.GetInt("LevelCompleted");

        Debug.Log("Level Completed: " + levelCompleted);

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
