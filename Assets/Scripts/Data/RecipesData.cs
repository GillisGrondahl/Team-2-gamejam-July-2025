using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Recipes", menuName = "ScriptableObjects/RecipesData", order = 1)]
public class RecipesData : ScriptableObject
{
    public List<RecipeData> Recipes = new List<RecipeData>();
    public RecipeData CurrentRecipe { get; set; } = null;

    public float Quantity = 0f;



}
