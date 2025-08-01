using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
public class RecipeSystem : MonoBehaviour
{
    public static RecipeSystem Instance { get; private set; }

    [SerializeField] private MMF_Player _MMFRecipeCompleted;

    public int wrongIngredientPenalty = 10;
    public int excessIngredientPenalty = 10;

    [SerializeField] TMP_Text recipeNameText = null;
    [SerializeField] Transform ingredientsList = null;
    [SerializeField] Transform ingredientUI = null;
    [SerializeField] TMP_Text qualityText = null;
    [SerializeField] MMProgressBar qualityBar = null;


    public List<RecipeData> recipes = null;
    [SerializeField] private int qualityOfCurrentRecipe = 100;
    private RecipeData _currentRecipe = null;
    private List<IngredientData> _currentIngredients = new List<IngredientData>();
    private Dictionary<IngredientData, int> _requiredIngredientsCount = new Dictionary<IngredientData, int>();
    private int _currentRecipeIndex = 0;
    private List<float> _qualityList = new List<float>();

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

    private void Start()
    {
        if (recipes.Count > 0)
        {
            _currentRecipe = recipes[_currentRecipeIndex]; // Start with the first recipe
        }
        //_compareRecipe = ScriptableObject.CreateInstance<RecipeData>();
        _requiredIngredientsCount = _currentRecipe.ingredients
            .GroupBy(i => i)
            .ToDictionary(g => g.Key, g => g.Count());
        UpdateUI();
    }

    private void UpdateUI()
    {
        // Update recipe name
        recipeNameText.text = _currentRecipe.recipeName;

        UpdateQualityText();
        foreach (Transform child in ingredientsList)
        {
            Destroy(child.gameObject);
        }

        foreach (var ingredient in _currentRecipe.ingredients)
        {
            Instantiate(ingredientUI, ingredientsList).GetComponent<IngredientUI>().Initialize(ingredient);
        }
    }

    public void AddIngredient(IngredientData ingredient)
    {
        _currentIngredients.Add(ingredient);

        if (!_requiredIngredientsCount.ContainsKey(ingredient))
        {
            qualityOfCurrentRecipe -= wrongIngredientPenalty;
            Debug.Log($"'{ingredient}' is not in recipe. -{wrongIngredientPenalty}%");
        }
        else
        {
            int currentCount = _currentIngredients.Count(i => i == ingredient);
            int allowedCount = _requiredIngredientsCount[ingredient];
            if (currentCount > allowedCount)
            {
                qualityOfCurrentRecipe -= excessIngredientPenalty;
                Debug.Log($"Too many '{ingredient}'! Allowed: {allowedCount}, now: {currentCount} -{excessIngredientPenalty}%");
            }
        }


        UpdateQualityText();
        CheckRecipeCompletion();
    }

    private void UpdateQualityText()
    {
        //qualityText.text = $"Quality:\n{qualityOfCurrentRecipe:F2}%";
        qualityBar.UpdateBar(qualityOfCurrentRecipe / 100f, 0f, 1f);
    }

    private void AddAndResetQuality()
    {
        _qualityList.Add(qualityOfCurrentRecipe);
        qualityOfCurrentRecipe = 100;
    }

    private void CheckRecipeCompletion()
    {
        bool missingIngredient = false;
        foreach (var ingredient in _requiredIngredientsCount)
        {
            if (_currentIngredients.Count(i => i == ingredient.Key) < ingredient.Value)
            {
                Debug.Log($"Recipe not complete: Missing {ingredient.Value - _currentIngredients.Count(i => i == ingredient.Key)} of {ingredient.Key.ingredientName}.");
                missingIngredient = true;
            }
        }

        if (missingIngredient) return;

        AddAndResetQuality();

        if (++_currentRecipeIndex < recipes.Count)
        {
            _currentRecipe = recipes[_currentRecipeIndex];
            _currentIngredients.Clear();
            _requiredIngredientsCount = _currentRecipe.ingredients
                .GroupBy(i => i)
                .ToDictionary(g => g.Key, g => g.Count());
            
            UpdateUI();
            Debug.Log($"Recipe complete! Moving to next recipe: {_currentRecipe.recipeName}");

            // Call MMF feedback for recipe completed
            _MMFRecipeCompleted.PlayFeedbacks();
        }
        else
        {
            float overallQuality = _qualityList.Average();


            Debug.Log($"LEVEL FINISHED! Quality: {overallQuality}%");
        }

    }
}
