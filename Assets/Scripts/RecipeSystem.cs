using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class RecipeSystem : IStartable
{
    public float OverallQuality { get => _qualityList.Average(); }
    public int QualityOfCurrentRecipe { get; private set; } = 100;
    public int CurrentRecipeIndex { get; private set; } = 0;
    public int TotalRecipes => _recipes?.Count ?? 0;

    public event Action<RecipeData> NewRecipe;
    public event Action<int> RecipeUpdated;
    public event Action RecipeCompleted;
    public event Action AllRecipesCompleted;

    public event Action NotIngredientAdded;
    public event Action GoodIngredientAdded;
    public event Action BadIngredientAdded;


    private List<RecipeData> _recipes = null;
    private List<RecipeStep> _currentlyAddedIngredients = new();
    private Dictionary<IngredientData, int> _requiredIngredientsCount = new Dictionary<IngredientData, int>();
    private List<float> _qualityList = new List<float>();
    private RecipeData _currentRecipe = null;

    private readonly LevelData _levelData;

    public RecipeSystem(LevelData LevelData)
    {
        _levelData = LevelData;
    }

    public void Start()
    {
        GetRecipes();
    }

    private void GetRecipes()
    {
        _recipes = _levelData.levelRecipes;

        if (_recipes.Count > 0)
            GetNextRecipe();
    }

    private void GetNextRecipe()
    {
        _currentRecipe = ScriptableObject.Instantiate(_recipes[CurrentRecipeIndex]);
        _requiredIngredientsCount = _currentRecipe.RequiredIngredients
            .GroupBy(i => i.Ingredient)
            .ToDictionary(g => g.Key, g => g.Count());

        NewRecipe?.Invoke(_currentRecipe);
    }

    public void AddNotIngredient()
    {
        QualityOfCurrentRecipe -= _levelData.wrongIngredientPenalty;
        NotIngredientAdded?.Invoke();
    }


    public void AddIngredient(IngredientData ingredient, int amountOfpices)
    {
        var currentPosition = new RecipeStep(ingredient, amountOfpices, false);
        _currentlyAddedIngredients.Add(currentPosition);


        if (!_currentRecipe.RequiredIngredients.Any(step => step.Ingredient == currentPosition.Ingredient))
        {
            QualityOfCurrentRecipe -= _levelData.wrongIngredientPenalty;
            Debug.Log($"'{ingredient}' is not in recipe. -{_levelData.wrongIngredientPenalty}%");
            BadIngredientAdded?.Invoke();
        }
        else
        {
            int currentCount = _currentlyAddedIngredients.Count(step => step.Ingredient == ingredient);
            int allowedCount = _recipes[CurrentRecipeIndex].RequiredIngredients.Count(step => step.Ingredient == ingredient);//_requiredIngredientsCount[ingredient];

            var allowedIngredient = _currentRecipe.RequiredIngredients.FirstOrDefault(i => i.Ingredient == ingredient && !i.IsDone);
            int allowedAmountOfPices = allowedIngredient?.PiecesCount ?? 1;

            var recipePosition = _currentRecipe.RequiredIngredients.FirstOrDefault(p => p.Ingredient == ingredient
            && p.PiecesCount == amountOfpices
            && !p.IsDone);

            if (recipePosition != null)
            {
                GoodIngredientAdded?.Invoke();
                recipePosition.IsDone = true;
            }
            else if (amountOfpices != allowedAmountOfPices)
            {
                QualityOfCurrentRecipe -= _levelData.excessIngredientPenalty;
                Debug.Log($"Wrong amount of '{ingredient}' pices! Allowed: {allowedAmountOfPices}, now: {amountOfpices} -{_levelData.excessIngredientPenalty}%");
                BadIngredientAdded?.Invoke();
            }
            else if (currentCount > allowedCount)
            {
                QualityOfCurrentRecipe -= _levelData.excessIngredientPenalty;
                Debug.Log($"Too many '{ingredient}'! Allowed: {allowedCount}, now: {currentCount} -{_levelData.excessIngredientPenalty}%");
                BadIngredientAdded?.Invoke();
            }

        }

        RecipeUpdated?.Invoke(QualityOfCurrentRecipe);
        CheckRecipeCompletion();
    }



    private void CheckRecipeCompletion()
    {
        //bool missingIngredient = false;

        //missingIngredient = _currentRecipe.RequiredIngredients.Any(step => step.IsDone == false);

        //foreach (var ingredient in _requiredIngredientsCount)
        //{
        //    if (_currentIngredients.Count(p => p.Ingredient == ingredient.Key) < ingredient.Value)
        //    {
        //        Debug.Log($"Recipe not complete: Missing {ingredient.Value - _currentIngredients.Count(p => p.Ingredient == ingredient.Key)} of {ingredient.Key.ingredientName}.");
        //        missingIngredient = true;
        //    }
        //}

        //if (_currentRecipe.RequiredIngredients.All(p => p.IsDone))
        //    missingIngredient = false;

        //if (missingIngredient) return;

        if (_currentRecipe.RequiredIngredients.Any(step => step.IsDone == false)) return;

        AddAndResetQuality();

        RecipeCompleted?.Invoke();

        if (++CurrentRecipeIndex < _recipes.Count)
        {
            GetNextRecipe();
            //Debug.Log($"Recipe complete! Moving to next recipe: {_recipes[CurrentRecipeIndex].recipeName}");

        }
        else
        {
            AllRecipesCompleted?.Invoke();
        }
    }

    private void AddAndResetQuality()
    {
        _qualityList.Add(QualityOfCurrentRecipe);
        QualityOfCurrentRecipe = 100;
    }
}

