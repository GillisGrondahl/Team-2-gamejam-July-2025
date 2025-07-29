using UnityEngine;

public class IngredientChecker : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Ingredient>(out var ingredient))
        {
            if (ingredient.IsAPart) return;
            RecipeSystem.Instance.AddIngredient(ingredient.ingredient);
            Destroy(ingredient.gameObject);
        }
    }
}
