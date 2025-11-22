using UnityEngine;
using VContainer;

public class Cheats : MonoBehaviour
{
    RecipeSystem _recipeSystem;

    int levelCompleted = 0;

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
        //if (!showOverlay) return;

        // Simple window/box area
        GUILayout.BeginArea(new Rect(10, 10, 250, 150), "Debug", GUI.skin.window);

        // Example label showing some value from your script
            GUILayout.Label("Level Completed: " + levelCompleted);
        //if (target != null)
        //{
        //    GUILayout.Label("Health: " + target.health);
        //    GUILayout.Label("Score: " + target.score);
        //}
        //else
        //{
        //    GUILayout.Label("No target assigned");
        //}

        // Button that calls a method on your script
        if (GUILayout.Button("Finish All Recipes"))
        {
            FinishLevel();
        }

        GUILayout.EndArea();
    }

}
