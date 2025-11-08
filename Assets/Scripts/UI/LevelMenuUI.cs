using System;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class LevelMenuUI : MonoBehaviour
{

    [SerializeField] private GameObject levelMenu;

    LevelManager _levelManager;

    [Inject]
    public void Construct(LevelManager levelManager)
    {
        _levelManager = levelManager;
    }

    private void OnEnable()
    {
        _levelManager.GamePaused += ShowLevelMenuUI;
        _levelManager.GameResumed += HideLevelMenuUI;
    }

    private void OnDisable()
    {
        _levelManager.GamePaused -= ShowLevelMenuUI;
        _levelManager.GameResumed -= HideLevelMenuUI;
    }

    public void ResumeButtonClicked()
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
