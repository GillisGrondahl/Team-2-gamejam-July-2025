using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelSelectionManager : MonoBehaviour
{
    [SerializeField] private List<StepsActivator> levelStepsActivators = new();

    [SerializeField] private int levelCompleted = 0;

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
            else if(levelCompleted == i)
            {
                levelStepsActivators[i].ShowStepsSequentially();
            }
            else
            {
                levelStepsActivators[i].HideSteps();
            }
        }
    }
}
