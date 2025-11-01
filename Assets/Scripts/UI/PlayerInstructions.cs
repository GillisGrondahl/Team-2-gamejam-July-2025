using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class PlayerInstructions : MonoBehaviour
{
    [SerializeField] private Button _continueButton;
    [SerializeField] private MMF_Player _recipeArrow;
    [SerializeField] private MMF_Player _countdownArrow;
    private bool _recipeArrowPlayed = false;
    private bool _countdownArrowPlayed = false;

    private ITimerService _timer;

    [Inject]
    private void Cunstruct(ITimerService timeManager)
    {
        _timer = timeManager;
    }


    private void Start()
    {
        _timer.Pause();

        _continueButton.onClick.AddListener(() =>
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
                gameObject.SetActive(false);
                _timer.Resume();
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        });
    }
   



}
