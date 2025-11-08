using UnityEngine;


public class LevelDifficultyManager : MonoBehaviour
{
    public static LevelDifficultyManager Instance { get; private set; }

    public LevelData levelDifficultyData;

    public bool landlubberMode = false;  // landlubber mode: accessability setting where the ship movement is frozen

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