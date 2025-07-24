using UnityEngine;

public class IngredientSpawner : MonoBehaviour
{
    [SerializeField] private Ingredient IngredientPrefab;


    public void SpawnIngredient(Interactor interactor)
    {
        var ingredientInstance = Instantiate(IngredientPrefab, interactor.transform.position, interactor.transform.rotation);
        ingredientInstance.PickUp(interactor);
    }
}
