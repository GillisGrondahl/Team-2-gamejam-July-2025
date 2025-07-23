using UnityEngine;

public class IngredientChecker : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Ingredient>(out var interactable))
        {
            RecipeSystem.Instance.AddIngredient(interactable.ingredient);
            Destroy(interactable.gameObject);
        }
    }
}
