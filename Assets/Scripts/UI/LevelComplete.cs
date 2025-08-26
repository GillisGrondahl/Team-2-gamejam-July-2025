using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelComplete : MonoBehaviour
{
    [SerializeField] private float _scoreToComplete = 0.6f;
    [SerializeField] private Sprite _starOutline, _starFilled;
    [SerializeField] private Image _star1, _star2, _star3, _star4, _star5;
    [SerializeField] private TMP_Text _evaluationText, _NrMealsCompletedText;
    [SerializeField] private Button _continueButton, _retryButton;


    public void EvaluateScore(float score)
    {

        if (!Cursor.visible)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        if (score >= _scoreToComplete)
        {
            SetEvaluationText("brilliantly");
            _continueButton.interactable = true;
        }
        else
        {
            SetEvaluationText("miserably");
            _continueButton.interactable = false;
            _retryButton.interactable = true;
        }

        SetStars(Mathf.RoundToInt(score / 20f));

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

    public void SetNrMealsCompletedText(int mealsCompleted, int maxMeals)
    {
        _NrMealsCompletedText.text = "Meals completed: " + mealsCompleted.ToString() + "/" + maxMeals.ToString();
    }
}
