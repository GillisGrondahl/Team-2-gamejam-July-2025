using UnityEngine;

[CreateAssetMenu(fileName = "New Ingredient Data", menuName = "ScriptableObjects/IngredientData")]
public class IngredientData : ScriptableObject
{
    public string ingredientName;
    public Sprite icon;
}
