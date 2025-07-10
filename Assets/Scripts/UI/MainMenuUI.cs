using Assets.Scripts;
using System;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button exitButton;

    private void Awake()
    {
        startButton.onClick.AddListener(() =>
        {
            GameEventsOmni.instance.StartGamePressed();
        });
        exitButton.onClick.AddListener(() =>
        {
            Application.Quit();
        });
    }

}
