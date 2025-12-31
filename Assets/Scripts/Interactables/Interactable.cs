using System;
using UnityEngine;
using UnityEngine.Events;


public interface IInteractable
{
    void OnHoverStart(IInteractor interactor);
    void OnHoverEnd(IInteractor interactor);

    void OnInteractStart(IInteractor interactor);
    void OnInteractEnd(IInteractor interactor);
}

public interface IDespawnNotifiable
{
    event Action<IDespawnNotifiable> Despawned;
}

public class Interactable : MonoBehaviour, IInteractable, IDespawnNotifiable
{
    [field: SerializeField] public LayerMask OriginalLayer { get; set; }
    [field: SerializeField] public LayerMask OutlineLayer { get; set; }
    [SerializeField] Transform attachTransform;
    [SerializeField] bool pickable = true;

    [Header("Events")]
    public UnityEvent<Interactor> OnInteract = new UnityEvent<Interactor>();
    public UnityEvent<Interactor> OnStopInteract = new UnityEvent<Interactor>();

    public event Action<IDespawnNotifiable> Despawned;

    private void Awake()
    {
        gameObject.layer = GetLayerFromMask(OriginalLayer.value);
    }


    public void OnHoverStart(IInteractor interactor)
    {
        ShowOutline();
    }

    public void OnHoverEnd(IInteractor interactor)
    {
        HideOutline();
    }

    public void OnInteractStart(IInteractor interactor)
    {
        //interactor.OverlapedInteractable = this;
        OnInteract?.Invoke(interactor as Interactor);

        if (!pickable) return;

        if (attachTransform == null)
        {
            transform.rotation = interactor.SnapPoint.rotation;
            transform.position = interactor.SnapPoint.position;
        }
        else
        {
            // Compute the offset from the interactable's root to its attachTransform
            Quaternion toAttachRot = Quaternion.Inverse(transform.rotation) * attachTransform.rotation;
            Vector3 toAttachPos = attachTransform.position - transform.position;

            // Apply that offset in reverse from the target SnapPoint
            transform.rotation = interactor.SnapPoint.rotation * Quaternion.Inverse(toAttachRot);
            transform.position = interactor.SnapPoint.position - (transform.rotation * toAttachPos);
        }
    }


    public void OnInteractEnd(IInteractor interactor)
    {
        OnStopInteract?.Invoke(interactor as Interactor);
    }

    public void ShowOutline()
    {
        // if (gameObject != null)
        gameObject.layer = GetLayerFromMask(OutlineLayer.value);
        //for (int i = 0; i < gameObject.transform.childCount; i++)
        //{
        //    gameObject.transform.GetChild(i).gameObject.layer = GetLayerFromMask(OutlineLayer.value);
        //}
    }

    public void HideOutline()
    {
        // if (gameObject != null)
        gameObject.layer = GetLayerFromMask(OriginalLayer.value);
        //for (int i = 0; i < gameObject.transform.childCount; i++)
        //{
        //    gameObject.transform.GetChild(i).gameObject.layer = GetLayerFromMask(OriginalLayer.value);
        //}
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
        Despawned?.Invoke(this);
    }

}
