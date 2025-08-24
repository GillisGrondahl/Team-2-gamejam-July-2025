using MoreMountains.Feedbacks;
using UnityEngine;

public class IngredientChecker : MonoBehaviour
{
    [SerializeField] private MMF_Player _MMFIngredientDropInPot;
    [SerializeField] private MMF_Player _MMFToolDropInPot;

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;

        if (rb != null)
        {
            if (Vector3.Dot(rb.linearVelocity.normalized, Vector3.down) < 0.8f)
                return;

            if (other.TryGetComponent<Ingredient>(out var ingredient))
            {

                if (ingredient.IsAPart)
                {
                    ingredient = ingredient.ParentIngredient;
                }

                int pices = ingredient.GetComponentsInChildren<Ingredient>().Length;
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
}
