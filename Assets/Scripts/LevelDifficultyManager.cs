using UnityEngine;


public class LevelDifficultyManager : MonoBehaviour
{
    public static LevelDifficultyManager Instance { get; private set; }

    public LevelDifficultyData levelDifficultyData;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}