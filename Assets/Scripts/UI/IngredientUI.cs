using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IngredientUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text ingredientName;
    public void Initialize(IngredientData ingredient, int pices)
    {
        ingredientName.text = ingredient.ingredientName;
        icon.sprite = ingredient.icon;

        if (pices > 1)
        {
            ingredientName.text = $"{ingredient.ingredientName} cut in {pices} pices";
        }


    }
}
