using TMPro;
using UnityEngine;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using System.Collections.Generic;
using VContainer;

public class RecipeUI : MonoBehaviour
{
    [SerializeField] private MMF_Player _MMFRecipeCompleted;

    [SerializeField] TMP_Text recipeNameText = null;
    [SerializeField] Transform ingredientsList = null;
    [SerializeField] Transform ingredientUI = null;
    //[SerializeField] TMP_Text qualityText = null;
    [SerializeField] MMProgressBar qualityBar = null;


    private List<(RecipeStep recipe, IngredientUI ui)> _positionUiPairs = new();

    private RecipeSystem _recipeSystem;

    [Inject]
    private void Construct(RecipeSystem recipeSystem)
    {
        _recipeSystem = recipeSystem;
    }

    private void OnEnable()
    {
        _recipeSystem.NewRecipe += OnNewRecipe;
        _recipeSystem.RecipeUpdated += OnRecipeUpdate;
        _recipeSystem.RecipeCompleted += OnRecipeComplete;
    }


    private void OnDisable()
    {
        _recipeSystem.NewRecipe -= OnNewRecipe;
        _recipeSystem.RecipeUpdated -= OnRecipeUpdate;
        _recipeSystem.RecipeCompleted -= OnRecipeComplete;
    }

    private void OnNewRecipe(RecipeData recipe)
    {
        recipeNameText.text = recipe.recipeName;
        _positionUiPairs.Clear();

        foreach (Transform child in ingredientsList)
        {
            Destroy(child.gameObject);
        }

        foreach (var position in recipe.RequiredIngredients)
        {
            var ui = Instantiate(ingredientUI, ingredientsList).GetComponent<IngredientUI>();
            ui.Initialize(position.Ingredient, position.PiecesCount);

            _positionUiPairs.Add((position, ui));
        }

        OnRecipeUpdate(100);
    }


    private void OnRecipeUpdate(int quality)
    {
        qualityBar.UpdateBar(quality / 100f, 0f, 1f);

        foreach (var (recipe, ui) in _positionUiPairs)
        {
            ui.SetTickMark(recipe.IsDone);
        }
    }
    private void OnRecipeComplete()
    {
        _MMFRecipeCompleted.PlayFeedbacks();
    }
}
