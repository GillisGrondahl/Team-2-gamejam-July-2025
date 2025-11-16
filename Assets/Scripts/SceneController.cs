using MoreMountains.Feedbacks;
using UnityEngine;

public class SceneController : MonoBehaviour, ISceneController
{

    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string levelSelectionSceneName = "LevelSelection";
    [SerializeField] private MMF_Player transitionPlayer;
    private MMF_LoadScene _sceneLoadFeedback;

    private void Awake()
    {
        _sceneLoadFeedback = transitionPlayer.GetFeedbackOfType<MMF_LoadScene>();

        if (_sceneLoadFeedback == null)
        {
            Debug.LogWarning("SceneTransitionController: No MMF_SceneLoad found in this MMF_Player.");
        }
    }

    private void PlayTransitionToScene(string sceneName)
    {
        _sceneLoadFeedback.DestinationSceneName = sceneName;
        transitionPlayer.PlayFeedbacks();
    }

    public void LoadLevelSelection()
    {
        PlayTransitionToScene(levelSelectionSceneName);
    }

    public void LoadScene(string sceneName)
    {
        PlayTransitionToScene(sceneName);
    }

    public void RetryCurrentLevel()
    {
        string current = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        PlayTransitionToScene(current);
    }

    public void LoadMainMenu()
    {
        PlayTransitionToScene(mainMenuSceneName);
    }

}
