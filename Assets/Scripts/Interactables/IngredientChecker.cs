using MoreMountains.Feedbacks;
using UnityEngine;

public class IngredientChecker : MonoBehaviour
{
    [SerializeField] private MMF_Player _MMFDropInPot;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Ingredient>(out var interactable))
        {
            RecipeSystem.Instance.AddIngredient(interactable.ingredient);
            Destroy(interactable.gameObject);

            // call MMF Feedback for playing sounds when dropping in pot
            _MMFDropInPot.PlayFeedbacks();
        }
    }
}
