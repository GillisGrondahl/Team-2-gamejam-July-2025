using MoreMountains.Feedbacks;
using System;
using UnityEngine;
using VContainer;

public class ParrotController : MonoBehaviour
{
    [SerializeField] private MMF_Player toolFeedback;
    [SerializeField] private MMF_Player badIngredientFeedback;
    [SerializeField] private MMF_Player goodIngredientFeedback;
    [SerializeField] private MMF_Player recipeCompletedFeedback;
    [SerializeField] private MMF_Player allRecipesCompletedFeedback;

    RecipeSystem _recipeSystem;

    [Inject]
    public void Construct(RecipeSystem recipeSystem)
    {
       _recipeSystem = recipeSystem;
    }

    private void OnEnable()
    {
        _recipeSystem.NotIngredientAdded += OnNotIngredientAdded;
        _recipeSystem.BadIngredientAdded += OnBadIngredientAdded;
        _recipeSystem.GoodIngredientAdded += OnGoodIngredientAdded;
        _recipeSystem.RecipeCompleted += OnRecipeCompleted;
        _recipeSystem.AllRecipesCompleted += OnAllRecipesCompleted;
    }

    private void OnDisable()
    {
        _recipeSystem.NotIngredientAdded -= OnNotIngredientAdded;
        _recipeSystem.BadIngredientAdded -= OnBadIngredientAdded;
        _recipeSystem.GoodIngredientAdded -= OnGoodIngredientAdded;
        _recipeSystem.RecipeCompleted -= OnRecipeCompleted;
        _recipeSystem.AllRecipesCompleted -= OnAllRecipesCompleted;
    }

    private void OnGoodIngredientAdded()
    {
        goodIngredientFeedback.PlayFeedbacks();
    }

    private void OnBadIngredientAdded()
    {
        badIngredientFeedback.PlayFeedbacks();
    }

    private void OnNotIngredientAdded()
    {
        toolFeedback.PlayFeedbacks();
    }

    private void OnRecipeCompleted()
    {
        recipeCompletedFeedback.PlayFeedbacks();
    }

    private void OnAllRecipesCompleted()
    {
        allRecipesCompletedFeedback.PlayFeedbacks();
    }
}
