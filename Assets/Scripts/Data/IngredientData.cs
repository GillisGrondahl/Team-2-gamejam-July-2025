using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Ingredient Data", menuName = "ScriptableObjects/IngredientData")]
[Serializable]
public class IngredientData : ScriptableObject
{
    public string ingredientName;
    public Sprite icon;
    public bool cut;
}
