using UnityEngine;
using VContainer;

public class Cheats : MonoBehaviour
{
    RecipeSystem _recipeSystem;

    int levelCompleted = 0;

#if UNITY_EDITOR || DEVELOPMENT_BUILD

    [Inject]
    private void Construct(RecipeSystem recipeSystem)
    {
        _recipeSystem = recipeSystem;
    }

    private void Start()
    {
        levelCompleted = PlayerPrefs.GetInt("LevelCompleted");
    }

    [ContextMenu("Finish Level")]
    private void FinishLevel()
    {
        _recipeSystem.FinishAllRecipes();
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 250, 150), "Debug", GUI.skin.window);
        GUILayout.Label("Level Completed: " + levelCompleted);

        if (GUILayout.Button("Finish All Recipes"))
        {
            FinishLevel();
        }

        GUILayout.EndArea();
    }

#endif

}
