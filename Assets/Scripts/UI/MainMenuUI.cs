using System;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;


    private void Awake()
    {
        startButton.onClick.AddListener(() =>
        {
            GameEvents.Instance.OnStartGameClicked?.Invoke();
        });

        settingsButton.onClick.AddListener(() => {
            GameEvents.Instance.OnSettingsClicked?.Invoke();
        });

        exitButton.onClick.AddListener(() =>
        {
            GameEvents.Instance.OnExitGameClicked?.Invoke();
        });
    }




}
