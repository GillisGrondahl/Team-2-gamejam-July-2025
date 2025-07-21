using System;
using System.Collections.Generic;
using UnityEngine;
public class RecipeSystem : MonoBehaviour
{
    public static RecipeSystem Instance { get; private set; }

    [SerializeField] Transform ingredientsList;
    [SerializeField] Transform ingredientUI;

    public List<Recipe> recipes;
    private Recipe currentRecipe;
    private Recipe compareRecipe;
    private int currentRecipeIndex = 0;

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
            currentRecipe = recipes[currentRecipeIndex]; // Start with the first recipe
        }
        compareRecipe = ScriptableObject.CreateInstance<Recipe>();
        UpdateUI();
    }

    private void UpdateUI()
    {
        foreach (Transform child in ingredientsList)
        {
            Destroy(child.gameObject);
        }

        foreach (var ingredient in currentRecipe.ingredients)
        {
            Instantiate(ingredientUI, ingredientsList).GetComponent<IngredientUI>().Initialize(ingredient);
        }
    }

    public void AddIngredient(Ingredient ingredient)
    {
        if (currentRecipe == null) return;

        compareRecipe.AddIngredient(ingredient);

        Debug.Log($"Added ingredient: {ingredient.ingredientName}");
        CheckRecipeCompletion();
    }

    private void CheckRecipeCompletion()
    {
        if (currentRecipe == null || compareRecipe == null) return;

        if (compareRecipe.ingredients.Count != currentRecipe.ingredients.Count)
        {
            Debug.Log("Recipe not complete: Ingredient count mismatch.");
            return;
        }

        foreach (var ingredient in currentRecipe.ingredients)
        {
            if (!compareRecipe.ingredients.Contains(ingredient))
            {
                Debug.Log($"Recipe not complete: Missing ingredient {ingredient.ingredientName}.");
                return;
            }
        }
        Debug.Log("Before check");

        if (++currentRecipeIndex < recipes.Count)
        {
            Debug.Log("InCheck");
            currentRecipe = recipes[currentRecipeIndex];
            compareRecipe.ClearIngredients();
            UpdateUI();

            Debug.Log($"Recipe complete! Moving to next recipe: {currentRecipe.recipeName}");
        }
        else
        {
            Debug.Log($"LEVEL FINISHED!");
        }

    }
}
