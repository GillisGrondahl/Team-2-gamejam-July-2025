using MoreMountains.Feedbacks;
using UnityEngine;

public class Countdown : MonoBehaviour
{
    [SerializeField] private MMF_Player _countdownReachedFeedback;
    [SerializeField] private MMF_Player _countdownTickFeedback;
    [SerializeField] private MMF_Player _timeUpFeedback;

    private void Start()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnFinalCountdownReached += OnCountdownReached;
            TimeManager.Instance.OnFinalCountdownTick += OnCountdownTick;
            TimeManager.Instance.OnTimeUp += OnTimeUp;
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
        if (_timeUpFeedback == null) return;
        _timeUpFeedback.PlayFeedbacks();
    }
}
