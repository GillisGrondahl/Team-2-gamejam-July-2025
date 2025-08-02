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
            if (ingredient.IsAPart)
                ingredient = ingredient.ParentIngredient;
            RecipeSystem.Instance.AddIngredient(ingredient.ingredient);
            Destroy(ingredient.gameObject);

            // call MMF Feedback for playing sounds when dropping in pot
            _MMFIngredientDropInPot.PlayFeedbacks();
        }
        else if (other.TryGetComponent<Tool>(out var tool))
        {
            RecipeSystem.Instance.AddIngredient(ScriptableObject.CreateInstance<IngredientData>());
            
            _MMFToolDropInPot.PlayFeedbacks();
            tool.ResetPosition();
        }
    }
}
