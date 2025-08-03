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
    private List<IngredientData> _currentIngredients = new List<IngredientData>();
    private Dictionary<IngredientData, int> _requiredIngredientsCount = new Dictionary<IngredientData, int>();
    private int _currentRecipeIndex = 0;
    private List<float> _qualityList = new List<float>();

    private List<RecipePosition> currentRecipe = new();

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
        TimeManager.Instance.OnTimeUp += OnTimerEnd;

        if (recipes.Count > 0)
            GetNewRecipe();
    }


    private void OnDestroy()
    {
            TimeManager.Instance.OnTimeUp -= OnTimerEnd;
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

        UpdateQualityText();
        foreach (Transform child in ingredientsList)
        {
            Destroy(child.gameObject);
        }

        foreach (var position in currentRecipe)
        {
            Instantiate(ingredientUI, ingredientsList).GetComponent<IngredientUI>()
                .Initialize(position.Ingredient, position.PicesCount);
        }
    }

    public void AddIngredient(IngredientData ingredient, int amountOfpices)
    {
        _currentIngredients.Add(ingredient);

        Debug.Log(ingredient.ingredientName + " added to recipe.");
        if (!_requiredIngredientsCount.ContainsKey(ingredient))
        {
            qualityOfCurrentRecipe -= wrongIngredientPenalty;
            Debug.Log($"'{ingredient}' is not in recipe. -{wrongIngredientPenalty}%");
        }
        else
        {
            int currentCount = _currentIngredients.Count(i => i == ingredient);
            int allowedCount = _requiredIngredientsCount[ingredient];

            int allowedAmountOfPices = currentRecipe.
                FirstOrDefault(i => i.Ingredient == ingredient && !i.IsDone).PicesCount;

            if (currentCount > allowedCount || amountOfpices != allowedAmountOfPices)
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
        levelComplete.SetNrMealsCompletedText(_currentRecipeIndex, recipes.Count);


        if (allRecipesDone)
        {
            OverallQuality = _qualityList.Average();

            levelComplete.EvaluateScore(OverallQuality);

            Debug.Log($"LEVEL FINISHED! Quality: {OverallQuality}%");
        }
        else
        {
            levelComplete.EvaluateScore(0f);

            Debug.Log("LEVEL FAILED!");
        }
    }

    private void OnTimerEnd()
    {
        EndLevel(false);
    }

    private struct RecipePosition
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
