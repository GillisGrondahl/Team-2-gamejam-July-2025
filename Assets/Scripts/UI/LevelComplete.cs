using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelComplete : MonoBehaviour
{
    [SerializeField] private Sprite _starOutline, _starFilled;
    [SerializeField] private Image _star1, _star2, _star3, _star4, _star5;
    [SerializeField] private TMP_Text _evaluationText, _timeCompleted;
    [SerializeField] private Button _continueButton, _retryButton;


    public void EvaluateScore(float time)
    {

        if (!Cursor.visible)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        if (time <= LevelDifficultyManager.Instance.levelDifficultySO.timeToComplete5Star)
        {
            SetStars(5);
            SetEvaluationText("brilliantly");
        }
        else if (time <= LevelDifficultyManager.Instance.levelDifficultySO.timetoComplete4Star)
        {
            SetStars(4);
            SetEvaluationText("brilliantly");
        }
        else if (time <= LevelDifficultyManager.Instance.levelDifficultySO.timeToComplete3Star)
        {
            SetStars(3);
            SetEvaluationText("mediocre");
        }
        else if (time <= LevelDifficultyManager.Instance.levelDifficultySO.timeToComplete2Star)
        {
            SetStars(2);
            SetEvaluationText("miserably");
        }
        else
        {
            SetStars(1);
            SetEvaluationText("miserably");
        }

        _continueButton.interactable = true; // progress no matter the time
        _retryButton.interactable = true;

        SetTimeCompletedText();
    }

    public void SetEvaluationText(string text)
    {

        _evaluationText.text = text;
    }

    public void SetStars(int stars)
    {
        _star1.sprite = stars >= 1 ? _starFilled : _starOutline;
        _star2.sprite = stars >= 2 ? _starFilled : _starOutline;
        _star3.sprite = stars >= 3 ? _starFilled : _starOutline;
        _star4.sprite = stars >= 4 ? _starFilled : _starOutline;
        _star5.sprite = stars >= 5 ? _starFilled : _starOutline;
    }


    public void SetTimeCompletedText()
    {
        _timeCompleted.text = TimeManager.Instance.GetFormattedTime();
    }
}
