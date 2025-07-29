using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "SceneLoader", menuName = "ScriptableObjects/SceneLoader")]
public class SceneLoader : ScriptableObject
{
    [SerializeField] private int _id = 0;
    public void LoadSceneByID(int id)
    {
        _id = id;
        LoadScene();
    }

    public async void LoadScene()
    {
        await SceneManager.LoadSceneAsync(_id);
    }


}
