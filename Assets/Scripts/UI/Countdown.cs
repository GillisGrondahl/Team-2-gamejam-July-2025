using MoreMountains.Feedbacks;
using UnityEngine;

public class Countdown : MonoBehaviour
{
    [SerializeField] private MMF_Player _countdownReachedFeedback;
    [SerializeField] private MMF_Player _countdownTickFeedback;

    private void Start()
    {
        GameEvents.Instance.TimeManagerInstantiated += HandleTimeManagerInitiated;

    }

    private void HandleTimeManagerInitiated(TimeManager timeManager)
    {
        if (timeManager != null)
        {
            timeManager.OnFinalCountdownReached += OnCountdownReached;
            timeManager.OnFinalCountdownTick += OnCountdownTick;
            timeManager.OnTimeUp += OnTimeUp;
        }
    }

    private void OnCountdownReached()
    {
        if (_countdownReachedFeedback == null) return;
        _countdownReachedFeedback.PlayFeedbacks();
    }

    private void OnCountdownTick()
    {
        if (_countdownTickFeedback == null) return;
        _countdownTickFeedback.PlayFeedbacks();
    }

    private void OnTimeUp()
    {

    }
}
