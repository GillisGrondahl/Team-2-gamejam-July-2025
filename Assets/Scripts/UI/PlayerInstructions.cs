using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInstructions : MonoBehaviour
{
    [SerializeField] private Button _continueButton;
    [SerializeField] private MMF_Player _recipeArrow;
    [SerializeField] private MMF_Player _countdownArrow;
    private bool _recipeArrowPlayed = false;
    private bool _countdownArrowPlayed = false;


    private void Start()
    {
        TimeManager.Instance.PauseGame();

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
                TimeManager.Instance.UnpauseGame();
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        });
    }
   



}
