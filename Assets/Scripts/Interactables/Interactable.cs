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
    [SerializeField] Transform attachTransform;
    [SerializeField] bool pickable = true;

    [Header("Events")]
    public UnityEvent<Interactor> OnHover = new();
    public UnityEvent<Interactor> OnStopHover = new();
    public UnityEvent<Interactor> OnInteract = new();
    public UnityEvent<Interactor> OnStopInteract = new();

    public event Action<IDespawnNotifiable> Despawned;

    public void OnHoverStart(IInteractor interactor)
    {
        OnHover?.Invoke(interactor as Interactor);
    }

    public void OnHoverEnd(IInteractor interactor)
    {
        OnStopHover?.Invoke(interactor as Interactor);
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

    private void OnDestroy()
    {
        OnInteract.RemoveAllListeners();
        OnStopInteract.RemoveAllListeners();
        Despawned?.Invoke(this);
    }

}
