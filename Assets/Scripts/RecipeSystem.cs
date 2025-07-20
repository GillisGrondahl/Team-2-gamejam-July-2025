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
            currentRecipe = recipes[0]; // Start with the first recipe
        }
        compareRecipe = ScriptableObject.CreateInstance<Recipe>();
        foreach(var ingredient in currentRecipe.ingredients)
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
        if(currentRecipe == null || compareRecipe == null) return;

        if(compareRecipe.ingredients.Count != currentRecipe.ingredients.Count)
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
    }
}
