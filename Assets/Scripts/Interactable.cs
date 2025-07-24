using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    [field: SerializeField] LayerMask OriginalLayer { get; set; }
    [field: SerializeField] LayerMask OutlineLayer { get; set; }
    [SerializeField] Transform attachTransform;

    [Header("Events")]
    public UnityEvent<Interactor> OnInteract;
    public UnityEvent<Interactor> OnStopInteract;

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
    }

    public void HideOutline()
    {
        gameObject.layer = GetLayerFromMask(OriginalLayer.value);
    }

    public int GetLayerFromMask(int mask)
    {
        int layer = 0;
        while ((mask >>= 1) != 0)
            layer++;
        return layer;
    }

}
