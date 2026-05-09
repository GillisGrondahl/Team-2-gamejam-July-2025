using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public interface IInteractable
{
    void CollectInteractionColliders(List<Collider> colliders);

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
    [SerializeField] Collider[] interactionColliders;
    [SerializeField] bool collectChildColliders = true;

    readonly List<Collider> _colliderBuffer = new();

    [Header("Events")]
    public UnityEvent<Interactor> OnHover = new();
    public UnityEvent<Interactor> OnStopHover = new();
    public UnityEvent<Interactor> OnInteract = new();
    public UnityEvent<Interactor> OnStopInteract = new();

    public event Action<IDespawnNotifiable> Despawned;

    public void CollectInteractionColliders(List<Collider> colliders)
    {
        if (colliders == null)
            return;

        if (TryGetComponent<InteractableGroupMember>(out var groupMember) &&
            groupMember.Group != null)
        {
            groupMember.Group.CollectInteractionColliders(colliders);
            return;
        }

        CollectOwnInteractionColliders(colliders);
    }

    public void CollectOwnInteractionColliders(List<Collider> colliders)
    {
        if (colliders == null)
            return;

        if (interactionColliders != null && interactionColliders.Length > 0)
        {
            for (int i = 0; i < interactionColliders.Length; i++)
            {
                if (interactionColliders[i] != null)
                    colliders.Add(interactionColliders[i]);
            }

            return;
        }

        if (collectChildColliders)
        {
            _colliderBuffer.Clear();
            GetComponentsInChildren<Collider>(true, _colliderBuffer);
            for (int i = 0; i < _colliderBuffer.Count; i++)
            {
                if (_colliderBuffer[i] != null)
                    colliders.Add(_colliderBuffer[i]);
            }

            return;
        }

        if (TryGetComponent<Collider>(out var ownCollider))
            colliders.Add(ownCollider);
    }

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
