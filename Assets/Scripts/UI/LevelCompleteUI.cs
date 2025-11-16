using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class LevelCompleteUI : MonoBehaviour
{

    [SerializeField] private MMF_Player _MMFLevelEnd;
    [SerializeField] private float _scoreToComplete = 0.6f;
    [SerializeField] private Canvas _canvas;
    [SerializeField] private Sprite _starOutline, _starFilled;
    [SerializeField] private Image _star1, _star2, _star3, _star4, _star5;
    [SerializeField] private TMP_Text _evaluationText, _NrMealsCompletedText;
    [SerializeField] private Button _continueButton, _retryButton;

    private LevelManager _levelManager;
    private RecipeSystem _recipeSystem;
    private ISceneController _sceneController;

    [Inject]
    private void Construct(LevelManager levelManager, RecipeSystem recipeSystem, ISceneController sceneController)
    {
        _levelManager = levelManager;
        _recipeSystem = recipeSystem;
        _sceneController = sceneController;
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

        SetNrMealsCompletedText(_recipeSystem.CurrentRecipeIndex, _recipeSystem.TotalRecipes);

        _MMFLevelEnd.PlayFeedbacks();

        EvaluateScore(_recipeSystem.OverallQuality);
        Debug.Log($"LEVEL FINISHED! Quality: {_recipeSystem.OverallQuality}%");
    }

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
