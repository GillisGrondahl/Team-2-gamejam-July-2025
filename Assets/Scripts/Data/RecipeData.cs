using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Recipe Data", menuName = "ScriptableObjects/RecipeData")]
public class RecipeData : ScriptableObject
{
    public string recipeName;
    public List<IngredientData> ingredients;


    public void AddIngredient(IngredientData ingredient)
    {
        if (ingredients == null)
        {
            ingredients = new List<IngredientData>();
        }

        ingredients.Add(ingredient);
    }

    public void ClearIngredients()
    {
        ingredients.Clear();
    }

}
