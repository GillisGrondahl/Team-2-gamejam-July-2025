using MoreMountains.Feedbacks;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class LevelCompleteUI : MonoBehaviour
{

    [SerializeField] MMF_Player _MMFLevelEnd;
    [SerializeField] float _scoreToComplete = 0.6f;
    [SerializeField] Canvas _canvas;
    [SerializeField] Sprite _starOutline, _starFilled;
    [SerializeField] Image _star1, _star2, _star3, _star4, _star5;
    [SerializeField] TMP_Text _evaluationText, _NrMealsCompletedText;
    [SerializeField] Button _continueButton, _retryButton;

    LevelManager _levelManager;
    RecipeSystem _recipeSystem;
    ISceneController _sceneController;
    IGameStateService _state;

    private static readonly StateMask MenuMask =
    StateMask.TimeScale |
    StateMask.CursorVisible |
    StateMask.CursorUnlocked;

    [Inject]
    private void Construct(LevelManager levelManager, RecipeSystem recipeSystem, ISceneController sceneController, IGameStateService stateService)
    {
        _levelManager = levelManager;
        _recipeSystem = recipeSystem;
        _sceneController = sceneController;
        _state = stateService;
    }

    private void Awake()
    {
        _canvas.enabled = false;
    }


    private void OnEnable()
    {
        _levelManager.LevelEnded += OnLevelEnd;
        _continueButton.onClick.AddListener(() => _sceneController.LoadLevelSelection());
        _retryButton.onClick.AddListener(() => _sceneController.RetryCurrentLevel());
    }

    private void OnDisable()
    {
        _levelManager.LevelEnded -= OnLevelEnd;
        _continueButton.onClick.RemoveAllListeners();
        _retryButton.onClick.RemoveAllListeners();
    }

    private void OnLevelEnd()
    {
        _canvas.enabled = true;
        _state.Register(this, MenuMask);

        SetNrMealsCompletedText(_recipeSystem.CurrentRecipeIndex, _recipeSystem.TotalRecipes);
        _MMFLevelEnd.PlayFeedbacks();
        EvaluateScore(_recipeSystem.OverallQuality);
    }

    public void EvaluateScore(float score)
    {
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
        var text = "";
        if (_levelManager.IsInfinite)
            text = $"Meals completed: {mealsCompleted}\nScore: {_levelManager.Score}";
        else
            text = $"Meals completed: {mealsCompleted}/ {maxMeals}";

        _NrMealsCompletedText.text = text;
    }

    private void OnDestroy()
    {
        _state.Unregister(this);
    }
}
