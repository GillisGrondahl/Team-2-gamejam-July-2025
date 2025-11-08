using System;
using UnityEngine;

[Serializable]
public class RecipeStep
{
    [SerializeField] private IngredientData ingredient;
    [SerializeField] private int piecesCount;
    [SerializeField] private bool isDone;

    public IngredientData Ingredient => ingredient;
    public int PiecesCount => piecesCount;
    public bool IsDone { get => isDone; set => isDone = value; }


    public RecipeStep(IngredientData ingredient, int piecesCount, bool isDone)
    {
        this.ingredient = ingredient;
        this.piecesCount = piecesCount;
        this.isDone = isDone;
    }
}
