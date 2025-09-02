using UnityEngine;

[CreateAssetMenu(fileName = "LevelDifficultyData", menuName = "ScriptableObjects/LevelDifficultyData")]

public class LevelDifficultyData : ScriptableObject
{
    public int timeToComplete = 120;
    [Tooltip("Speed multiplier for overall wave motion intensity (effects pitch amplitude & overall speed)")]
    [Range(0f, 2f)] public float waveMotionIntensity = 1f;

}