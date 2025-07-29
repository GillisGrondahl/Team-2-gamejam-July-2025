using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    [field: SerializeField] public LayerMask OriginalLayer { get; set; }
    [field: SerializeField] public LayerMask OutlineLayer { get; set; }
    [SerializeField] Transform attachTransform;

    [Header("Events")]
    public UnityEvent<Interactor> OnInteract = new UnityEvent<Interactor>();
    public UnityEvent<Interactor> OnStopInteract = new UnityEvent<Interactor>();

    private void Awake()
    {
        gameObject.layer = GetLayerFromMask(OriginalLayer.value);
      
    }
    public void Interact(Interactor interactor)
    {
        interactor.OverlapedInteractable = this;

        OnInteract?.Invoke(interactor);

        if (attachTransform == null) return;
        // Align the interactable so its attachTransform matches the interactor
        Vector3 worldPos = attachTransform.position;
        Quaternion worldRot = attachTransform.rotation;

        // Calculate the offset
        Quaternion rotationOffset = interactor.transform.rotation * Quaternion.Inverse(worldRot);
        Vector3 positionOffset = interactor.transform.position - worldPos;

        // Apply offset to the root of the interactable
        transform.rotation = rotationOffset * transform.rotation;
        transform.position += positionOffset;
    }

    public void StopInteract(Interactor interactor)
    {
        interactor.OverlapedInteractable = null;
        OnStopInteract?.Invoke(interactor);
    }

    public void ShowOutline()
    {

        gameObject.layer = GetLayerFromMask(OutlineLayer.value);
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            gameObject.transform.GetChild(i).gameObject.layer = GetLayerFromMask(OutlineLayer.value);
        }
    }

    public void HideOutline()
    {
        gameObject.layer = GetLayerFromMask(OriginalLayer.value);
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            gameObject.transform.GetChild(i).gameObject.layer = GetLayerFromMask(OriginalLayer.value);
        }
    }

    public int GetLayerFromMask(int mask)
    {
        int layer = 0;
        while ((mask >>= 1) != 0)
            layer++;
        return layer;
    }

    private void OnDestroy()
    {
        OnInteract.RemoveAllListeners();
        OnStopInteract.RemoveAllListeners();
    }

}
