using MoreMountains.Feedbacks;
using System;
using TMPro;
using UnityEngine;
using VContainer;

public class CountdownUI : MonoBehaviour
{
    [SerializeField] private TMP_Text timeDisplayText;
    [SerializeField] private MMF_Player _countdownReachedFeedback;
    [SerializeField] private MMF_Player _countdownTickFeedback;

    private bool _earlyWarningTriggered = false;
    private int _lastAnnouncedSecond = int.MinValue;

    private ITimerService _timerService;
    private LevelData _levelData;
    private AudioManager _audioManager;

    [Inject]
    private void Construct(ITimerService timerService, SceneController sceneController, AudioManager audioManager)
    {
        _timerService = timerService;
        _levelData = sceneController.CurrentLevelData;
        _audioManager = audioManager;
    }

    private void Start()
    {
        timeDisplayText.color = Color.black;
        timeDisplayText.text = GetFormattedTime(0f);
        _earlyWarningTriggered = false;
    }

    private void OnEnable()
    {
        _timerService.Tick += OnTimerTick;
    }

    private void OnDisable()
    {
        _timerService.Tick -= OnTimerTick;
    }

    private void OnTimerTick(float time)
    {
        ShowTime(time);

        if (!_earlyWarningTriggered && time <= _levelData.earlyWarningInSeconds)
        {
            _earlyWarningTriggered = true;
            ShowWarning();
            _audioManager.SetTempo(0.3f);
        }


        if (_levelData.finalCountdownInSeconds > 0)
        {
            int wholeSeconds = Mathf.CeilToInt(time);
            if (wholeSeconds != _lastAnnouncedSecond && wholeSeconds <= _levelData.finalCountdownInSeconds && wholeSeconds > 0)
            {
                _lastAnnouncedSecond = wholeSeconds;
                ShowTimeEnding();
                _audioManager.SetTempo(0.5f);
            }
        }
    }

    public void ShowTime(float timeInSeconds)
    {
        timeDisplayText.text = GetFormattedTime(timeInSeconds);
    }

    public void ShowWarning()
    {
        //throw new System.NotImplementedException();
    }

    public void ShowTimeEnding()
    {
        timeDisplayText.color = Color.red;
        _countdownReachedFeedback?.PlayFeedbacks();
        _countdownTickFeedback?.PlayFeedbacks();
    }

    public void ShowTimeUp()
    {
        // Optionally implement any UI changes when time is up
    }

    public string GetFormattedTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        return string.Format($"{minutes:00}:{seconds:00}");
    }
}
