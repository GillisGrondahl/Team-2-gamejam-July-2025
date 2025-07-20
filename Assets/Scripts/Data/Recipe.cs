using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Recipe", menuName = "ScriptableObjects/Recipe")]
public class Recipe : ScriptableObject
{
    public string recipeName;
    public List<Ingredient> ingredients;


    public void AddIngredient(Ingredient ingredient)
    {
        if (ingredients == null)
        {
            ingredients = new List<Ingredient>();
        }

        ingredients.Add(ingredient);
    }

    public void ClearIngredients()
    {
        ingredients.Clear();
    }

}
