using MoreMountains.Feedbacks;
using UnityEngine;

public class SceneController : MonoBehaviour, ISceneController
{

    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string levelSelectionSceneName = "LevelSelection";
    [SerializeField] private string gameplaySceneName = "Gameplay";
    [SerializeField] private MMF_Player transitionPlayer;
    private MMF_LoadScene _sceneLoadFeedback;

    public LevelData CurrentLevelData { get; private set; }

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

    public void LoadSceneByName(string sceneName)
    {
        PlayTransitionToScene(sceneName);
    }

    public void LoadSceneByLevelData(LevelData levelData)
    {
        CurrentLevelData = levelData;
        PlayTransitionToScene(levelData.sceneName);
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
