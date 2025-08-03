using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IngredientUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private Image tickMark;
    [SerializeField] private Image knife;
    [SerializeField] private TMP_Text ingredientName;
    public IngredientData Ingredient { get; private set; }
    public void Initialize(IngredientData ingredient, int pices)
    {
        Ingredient = ingredient;
        ingredientName.text = Ingredient.ingredientName;
        icon.sprite = Ingredient.icon;

        if (pices > 1)
        {
            ingredientName.text = $"{Ingredient.ingredientName} cut in {pices} pices";
            knife.gameObject.SetActive(true);
        }
    }

    public void SetTickMark(bool value)
    {
        tickMark.gameObject.SetActive(value);
    }

    public bool GetTickMarkStatus()
    {
        return tickMark.gameObject.activeSelf;
    }
}
