using UnityEngine;

public interface ITimerUI
{
    void ShowTime(float timeInSeconds);
    void ShowWarning();

    void ShowTimeEnding();

    void ShowTimeUp();
}
