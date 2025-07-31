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

        // Align the interactable so its attachTransform matches the interactor's SnapPoint
        Debug.Log($"{gameObject.name}: has attachTransform {attachTransform.name} and interactor {interactor.name} with snap point {interactor.SnapPoint.name}");

        // Compute the offset from the interactable's root to its attachTransform
        Quaternion toAttachRot = Quaternion.Inverse(transform.rotation) * attachTransform.rotation;
        Vector3 toAttachPos = attachTransform.position - transform.position;

        // Apply that offset in reverse from the target SnapPoint
        transform.rotation = interactor.SnapPoint.rotation * Quaternion.Inverse(toAttachRot);
        transform.position = interactor.SnapPoint.position - (transform.rotation * toAttachPos);


        //if (attachTransform == null) return;
        //// Align the interactable so its attachTransform matches the interactor
        //Debug.Log($"{gameObject.name}: has attachTransform {attachTransform.name} and interactor {interactor.name} with snap point {interactor.SnapPoint.name}");
        //Vector3 worldPos = attachTransform.position;
        //Quaternion worldRot = attachTransform.rotation;

        //// Calculate the offset
        //Quaternion rotationOffset = interactor.transform.rotation * Quaternion.Inverse(worldRot);
        //Vector3 positionOffset = interactor.transform.position - worldPos;

        //// Apply offset to the root of the interactable
        //transform.rotation = rotationOffset * transform.rotation;
        //transform.position += positionOffset;
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
