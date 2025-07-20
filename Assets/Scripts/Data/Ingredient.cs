using UnityEngine;

[CreateAssetMenu(fileName = "New Ingredient", menuName = "ScriptableObjects/Ingredient")]
public class Ingredient : ScriptableObject
{
    public string ingredientName;
    public Sprite icon;
}
