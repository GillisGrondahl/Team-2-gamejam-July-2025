using MoreMountains.Feedbacks;
using System;
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
    IGameStateService _state;


    private static readonly StateMask PlayerInstractionsMask =
    StateMask.TimeScale |
    StateMask.CursorVisible |
    StateMask.CursorUnlocked;

    [Inject]
    private void Construct(LevelManager levelManager, IGameStateService stateService)
    {
        _levelManager = levelManager;
        _state = stateService;
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
        _state.Register(this, PlayerInstractionsMask);
    }

    private void Hide()
    {
        _canvas.enabled = false;
        _state.Unregister(this);
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
            Hide();
        }
    }

    private void OnDestroy()
    {
        _state.Unregister(this);
    }
}
