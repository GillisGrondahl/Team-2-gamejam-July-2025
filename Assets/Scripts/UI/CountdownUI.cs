using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;
using VContainer;

public class CountdownUI : MonoBehaviour, ITimerUI
{
    [SerializeField] private TMP_Text timeDisplayText;
    [SerializeField] private MMF_Player _countdownReachedFeedback;
    [SerializeField] private MMF_Player _countdownTickFeedback;


    private void Start()
    {
        timeDisplayText.color = Color.black;
        timeDisplayText.text = GetFormattedTime(0f);
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
