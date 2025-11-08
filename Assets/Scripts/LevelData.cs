using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelDifficultyData", menuName = "ScriptableObjects/LevelDifficultyData")]

public class LevelData : ScriptableObject
{

    public bool showPlayerInstructions = false;

    public float levelDurationInSeconds = 60f;
    public float earlyWarningInSeconds = 10f;
    public int finalCountdownInSeconds = 5;

    [Header("Scoring Penalties")]
    public int wrongIngredientPenalty = 10;
    public int excessIngredientPenalty = 10;

    [Tooltip("Speed multiplier for overall wave motion intensity (effects pitch amplitude & overall speed)")]
    [Range(0f, 2f)] public float waveMotionIntensity = 1f;

    [Header("Level Recipes")]
    [Tooltip("List of recipes that need to be completed in this level")]
    public List<RecipeData> levelRecipes = new List<RecipeData>();

}