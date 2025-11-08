using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class PlayerInstructionsUI : MonoBehaviour
{
    [SerializeField] private Canvas _canvas;
    [SerializeField] private Button _continueButton;
    [SerializeField] private MMF_Player _recipeArrow;
    [SerializeField] private MMF_Player _countdownArrow;
    private bool _recipeArrowPlayed = false;
    private bool _countdownArrowPlayed = false;

    LevelManager _levelManager;

    [Inject]
    private void Construct(LevelManager levelManager)
    {
        _levelManager = levelManager;
    }

    private void Awake()
    {
        Hide();
    }

    private void OnEnable()
    {
        _levelManager.ShowPlayerInstructions += Show;
    }

    private void OnDisable()
    {
        _levelManager.ShowPlayerInstructions -= Show;
    }

    private void Show()
    {

        _canvas.enabled = true;
        //_continueButton.onClick.AddListener(() =>
        //{
        //    if (!_recipeArrowPlayed)
        //    {
        //        _recipeArrow.PlayFeedbacks();
        //        _recipeArrowPlayed = true;
        //    }
        //    else if (!_countdownArrowPlayed)
        //    {
        //        _countdownArrow.PlayFeedbacks();
        //        _countdownArrowPlayed = true;
        //    }
        //    else
        //    {
        //        gameObject.SetActive(false);
        //    }
        //});
    }

    private void Hide()
    {
        _canvas.enabled = false;
    }

    public void NextInstruction()
    {
        if (!_recipeArrowPlayed)
        {
            _recipeArrow.PlayFeedbacks();
            _recipeArrowPlayed = true;
        }
        else if (!_countdownArrowPlayed)
        {
            _countdownArrow.PlayFeedbacks();
            _countdownArrowPlayed = true;
        }
        else
        {
            _canvas.enabled = false;
            _levelManager.TogglePause();
        }
    }
}
