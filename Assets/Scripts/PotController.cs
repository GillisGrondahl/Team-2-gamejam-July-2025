using MoreMountains.Feedbacks;
using System;
using UnityEngine;
using VContainer;

public class PotController : MonoBehaviour
{
    [SerializeField] private MMF_Player recipeCompleted;

    RecipeSystem _recipeSystem;

    [Inject]
    public void Construct(RecipeSystem recipeSystem)
    {
        _recipeSystem = recipeSystem;
    }

    private void OnEnable()
    {
        _recipeSystem.RecipeCompleted += OnRecipeCompleted;
    }

    private void OnDisable()
    {
        _recipeSystem.RecipeCompleted -= OnRecipeCompleted;
    }

    private void OnRecipeCompleted()
    {
        recipeCompleted.PlayFeedbacks();
    }
}
