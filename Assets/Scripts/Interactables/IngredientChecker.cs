using MoreMountains.Feedbacks;
using UnityEngine;

public class IngredientChecker : MonoBehaviour
{
    [SerializeField] private MMF_Player _MMFIngredientDropInPot;
    [SerializeField] private MMF_Player _MMFToolDropInPot;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Ingredient>(out var ingredient))
        {
            int pices = 1;
            if (ingredient.IsAPart)
            {
                ingredient = ingredient.ParentIngredient;
                pices = ingredient.GetComponentsInChildren<Ingredient>().Length;
            }
            Debug.Log($"Ingredient {ingredient.ingredient.ingredientName} has {pices} pices.");
            RecipeSystem.Instance.AddIngredient(ingredient.ingredient, pices);
            Destroy(ingredient.gameObject);

            // call MMF Feedback for playing sounds when dropping in pot
            _MMFIngredientDropInPot.PlayFeedbacks();
        }
        else if (other.TryGetComponent<Tool>(out var tool))
        {
            RecipeSystem.Instance.AddIngredient(ScriptableObject.CreateInstance<IngredientData>(), 1);

            _MMFToolDropInPot.PlayFeedbacks();
            tool.ResetPosition();
        }
    }
}
