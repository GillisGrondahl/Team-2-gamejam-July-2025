using UnityEngine;

public class IngredientSpawner : MonoBehaviour, IInteractable
{
    [SerializeField] private Ingredient IngredientPrefab;
    [field: SerializeField] public LayerMask OutlineLayer { get; private set; }
    public LayerMask OriginalLayer { get; private set; }

    private LayerMask originalMask;

    private void Awake()
    {
        OriginalLayer = gameObject.layer;    
    }

    public IInteractable Use(Transform target)
    {
        var ingredientInstance = Instantiate(IngredientPrefab, target.position, target.rotation);
        return ingredientInstance.Use(target);
        
    }
    public void ShowOutline()
    {
        gameObject.layer = ((IInteractable)this).GetLayerFromMask(OutlineLayer.value);
    }
    public void HideOutline()
    {
        gameObject.layer = ((IInteractable)this).GetLayerFromMask(OriginalLayer.value);
    }
    public void Release()
    {
        
    }
}
