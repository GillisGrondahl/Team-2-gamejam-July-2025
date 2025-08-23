using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
public class RecipeSystem : MonoBehaviour
{
    public static RecipeSystem Instance { get; private set; }

    public float OverallQuality { get; private set; } = 100f;

    [SerializeField] private MMF_Player _MMFRecipeCompleted;
    [SerializeField] private MMF_Player _MMFLevelEnd;

    public int wrongIngredientPenalty = 10;
    public int excessIngredientPenalty = 10;

    [SerializeField] TMP_Text recipeNameText = null;
    [SerializeField] Transform ingredientsList = null;
    [SerializeField] Transform ingredientUI = null;
    [SerializeField] TMP_Text qualityText = null;
    [SerializeField] MMProgressBar qualityBar = null;

    [SerializeField] GameObject levelCompleteUI = null;


    public List<RecipeData> recipes = null;
    [SerializeField] private int qualityOfCurrentRecipe = 100;
    //private RecipeData _currentRecipe = null;
    //private List<IngredientData> _currentIngredients = new List<IngredientData>();
    private List<RecipePosition> _currentIngredients = new();
    private Dictionary<IngredientData, int> _requiredIngredientsCount = new Dictionary<IngredientData, int>();
    private int _currentRecipeIndex = 0;
    private List<float> _qualityList = new List<float>();

    private List<RecipePosition> currentRecipe = new();

    private List<(RecipePosition recipe, IngredientUI ui)> _positionUiPairs = new();

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
            GetNewRecipe();
    }




    private void GetNewRecipe()
    {
        currentRecipe = new List<RecipePosition>();
        foreach (var recipePosition in recipes[_currentRecipeIndex].requiredIngredientsPices.ToList())
        {
            currentRecipe.Add(new RecipePosition(recipePosition.Key, recipePosition.Value, false));
        }
        //_currentRecipe = recipes[_currentRecipeIndex];
        _currentIngredients.Clear();
        _requiredIngredientsCount = currentRecipe
            .GroupBy(i => i.Ingredient)
            .ToDictionary(g => g.Key, g => g.Count());
        UpdateUI();
    }

    private void UpdateUI()
    {
        // Update recipe name
        recipeNameText.text = recipes[_currentRecipeIndex].recipeName;
        _positionUiPairs.Clear();

        UpdateLevelProgressText();
        foreach (Transform child in ingredientsList)
        {
            Destroy(child.gameObject);
        }

        foreach (var position in currentRecipe)
        {
            var ui = Instantiate(ingredientUI, ingredientsList).GetComponent<IngredientUI>();
            ui.Initialize(position.Ingredient, position.PicesCount);

            _positionUiPairs.Add((position, ui));
        }
    }

    public void AddIngredient(IngredientData ingredient, int amountOfpices)
    {
        var currentPosition = new RecipePosition(ingredient, amountOfpices, false);
        _currentIngredients.Add(currentPosition);

        if (!_requiredIngredientsCount.ContainsKey(ingredient))
        {
            qualityOfCurrentRecipe -= wrongIngredientPenalty;
            Debug.Log($"'{ingredient}' is not in recipe. -{wrongIngredientPenalty}%");
        }
        else
        {
            int currentCount = _currentIngredients.Count(p => p.Ingredient == ingredient);
            int allowedCount = _requiredIngredientsCount[ingredient];

            var allowedIngredient = currentRecipe.FirstOrDefault(i => i.Ingredient == ingredient && !i.IsDone);
            int allowedAmountOfPices = allowedIngredient?.PicesCount ?? 1;

            var recipePosition = currentRecipe.FirstOrDefault(p => p.Ingredient == ingredient
            && p.PicesCount == amountOfpices
            && !p.IsDone);

            if (recipePosition != null)
            {
                recipePosition.IsDone = true;
            }
            else if (amountOfpices != allowedAmountOfPices)
            {
                qualityOfCurrentRecipe -= excessIngredientPenalty;
                Debug.Log($"Wrong amount of '{ingredient}' pices! Allowed: {allowedAmountOfPices}, now: {amountOfpices} -{excessIngredientPenalty}%");
            }
            else if (currentCount > allowedCount)
            {
                qualityOfCurrentRecipe -= excessIngredientPenalty;
                Debug.Log($"Too many '{ingredient}'! Allowed: {allowedCount}, now: {currentCount} -{excessIngredientPenalty}%");
            }

        }

        UpdateRecipeProgress();
        UpdateLevelProgressText();

        CheckRecipeCompletion();
    }

    private void UpdateLevelProgressText()
    {
        //qualityBar.UpdateBar(qualityOfCurrentRecipe / 100f, 0f, 1f);
        float recipeProgress = (float)_currentRecipeIndex / recipes.Count;
        qualityBar.UpdateBar(recipeProgress, 0f, 1f);
        Debug.Log($"{_currentRecipeIndex} / {recipes.Count}");
    }

    private void UpdateRecipeProgress()
    { 

        foreach (var (recipe, ui) in _positionUiPairs)
        {
            ui.SetTickMark(recipe.IsDone);
        }
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
            if (_currentIngredients.Count(p => p.Ingredient == ingredient.Key) < ingredient.Value)
            {
                Debug.Log($"Recipe not complete: Missing {ingredient.Value - _currentIngredients.Count(p => p.Ingredient == ingredient.Key)} of {ingredient.Key.ingredientName}.");
                missingIngredient = true;
            }
        }

        if (currentRecipe.All(p => p.IsDone))
            missingIngredient = false;

        if (missingIngredient) return;

        AddAndResetQuality();

        UpdateLevelProgressText();

        if (++_currentRecipeIndex < recipes.Count)
        {
            GetNewRecipe();
            Debug.Log($"Recipe complete! Moving to next recipe: {recipes[_currentRecipeIndex].recipeName}");

            // Call MMF feedback for recipe completed
            _MMFRecipeCompleted.PlayFeedbacks();
        }
        else
        {
            _MMFRecipeCompleted.PlayFeedbacks();
            EndLevel(true);

        }

    }

    private void EndLevel(bool allRecipesDone)
    {
        levelCompleteUI.SetActive(true);
        LevelComplete levelComplete = levelCompleteUI.GetComponent<LevelComplete>();

        _MMFLevelEnd.PlayFeedbacks();

        if (allRecipesDone)
        {
            OverallQuality = _qualityList.Average();

            levelComplete.EvaluateScore(TimeManager.Instance.currentTime);

            TimeManager.Instance.TogglePause();
            Debug.Log($"LEVEL FINISHED! Quality: {OverallQuality}%");

        }

    }

    private void OnTimerEnd()
    {
        EndLevel(false);
    }

    private class RecipePosition
    {
        public IngredientData Ingredient { get; private set; }
        public int PicesCount { get; private set; }
        public bool IsDone { get; set; }

        public RecipePosition(IngredientData ingredient, int picesCount, bool isDone)
        {
            Ingredient = ingredient;
            PicesCount = picesCount;
            IsDone = isDone;
        }
    }
}
