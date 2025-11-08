using MoreMountains.Feedbacks;
using System.Collections;
using UnityEngine;
using VContainer;

public class IngredientChecker : MonoBehaviour
{
    [SerializeField] private float _cooldownTime = 0.2f;
    private bool _cooldown = false;
    [SerializeField] private MMF_Player _MMFIngredientDropInPot;
    [SerializeField] private MMF_Player _MMFToolDropInPot;

    private RecipeSystem _recipeSystem;

    [Inject]
    private void Construct(RecipeSystem recipeSystem)
    {
        _recipeSystem = recipeSystem;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_cooldown) return;

        Rigidbody rb = other.attachedRigidbody;

        if (rb != null)
        {
            if (Vector3.Dot(rb.linearVelocity.normalized, Vector3.down) < 0.6f)
                return;
            
            if (other.TryGetComponent<Ingredient>(out var ingredient))
            {

                
                if (ingredient.ParentIngredient != null)
                {
                    ingredient = ingredient.ParentIngredient;
                }

                int pices = ingredient.ingredientParts.Count + 1;
                Debug.Log($"Ingredient {ingredient.ingredient.ingredientName} has {pices} pices.");
                _recipeSystem.AddIngredient(ingredient.ingredient, pices);
                Destroy(ingredient.gameObject);

                // call MMF Feedback for playing sounds when dropping in pot
                _MMFIngredientDropInPot.PlayFeedbacks();
            }
            else if (other.TryGetComponent<Tool>(out var tool))
            {
                _recipeSystem.AddIngredient(ScriptableObject.CreateInstance<IngredientData>(), 1);
                _MMFToolDropInPot.PlayFeedbacks();
                tool.ResetPosition();
            }

            StartCoroutine(CheckCooldown());
        }
    }

    IEnumerator CheckCooldown()
    {
        _cooldown = true;
        yield return new WaitForSeconds(_cooldownTime);
        _cooldown = false;
    }
}
