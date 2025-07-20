using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IngredientUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text ingredientName;
    public void Initialize(Ingredient ingredient)
    {
        ingredientName.text = ingredient.ingredientName;
        icon.sprite = ingredient.icon;
    }
}
