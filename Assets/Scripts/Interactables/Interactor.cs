using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;


public interface IInteractor
{
    Transform Transform { get; }
    Vector3 CurrentVelocity { get; }
    Transform SnapPoint { get; }
}

public readonly struct InteractionContext
{
    public readonly IInteractor Interactor;
    public readonly IInteractable Interactable;

    public InteractionContext(IInteractor interactor, IInteractable interactable)
    {
        Interactor = interactor;
        Interactable = interactable;
    }
}

[DefaultExecutionOrder(-80)]
public class Interactor : MonoBehaviour, IInteractor
{

    public Vector3 CurrentVelocity { get; private set; }
    public Transform Transform => transform;
    [field: SerializeField] public Transform SnapPoint { get; private set; }

    [SerializeField] Transform handTransform;
    [SerializeField] Vector3 offset;

    [SerializeField] float cooldown = 0.5f;
    bool _canInteract = true;
    HandFollower hand;
    Vector3 _lastPosition;

    List<Collider> handColliders = null;
    List<Collider> interactableColliders = new();

    readonly HashSet<IDespawnNotifiable> _despawnSubscriptions = new();
    HashSet<IInteractable> hoveredInteractables = new();
    public IInteractable Selected { get; private set; }
    public IInteractable Candidate { get; private set; }


    IInputService _input;

    [Inject]
    public void Construct(IInputService input)
    {
        _input = input;
    }

    private void OnEnable()
    {
        _input.Interact += Interact;
    }

    private void OnDisable()
    {
        _input.Interact -= Interact;
    }


    private void Start()
    {
        handColliders = handTransform.GetComponentsInChildren<Collider>(true).ToList();
        hand = handTransform.GetComponent<HandFollower>();
        _lastPosition = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.TryGetComponent<IInteractable>(out var interactable))
            return;

        hoveredInteractables.Add(interactable);

        if (interactable is IDespawnNotifiable despawn)
        {
            // Avoid double subscription
            if (_despawnSubscriptions.Add(despawn))
                despawn.Despawned += OnInteractableDespawned;
        }

        RefreshCandidate();


    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.gameObject.TryGetComponent<IInteractable>(out var interactable))
            return;

        hoveredInteractables.Remove(interactable);

        if (interactable is IDespawnNotifiable despawn)
        {
            if (_despawnSubscriptions.Remove(despawn))
                despawn.Despawned -= OnInteractableDespawned;
        }

        //if (ReferenceEquals(Candidate, interactable))
        //{
        //    Candidate.OnHoverEnd(this);
        //    Candidate = null;
        //}

        RefreshCandidate();


    }

    private void OnInteractableDespawned(IDespawnNotifiable despawned)
    {
        // Convert back to IInteractable
        var interactable = despawned as IInteractable;
        if (interactable == null)
            return;

        // Clean all references
        hoveredInteractables.Remove(interactable);

        if (ReferenceEquals(Candidate, interactable))
        {
            Candidate.OnHoverEnd(this);
            Candidate = null;
        }

        if (ReferenceEquals(Selected, interactable))
        {
            // End interaction safely
            Selected.OnInteractEnd(this);
            Selected = null;
            StartCoroutine(Cooldown());
        }

        // Unsubscribe defensively
        despawned.Despawned -= OnInteractableDespawned;
        _despawnSubscriptions.Remove(despawned);

        RefreshCandidate();
    }

    private void RefreshCandidate()
    {
        if (Selected != null)
            return;

        IInteractable best = null;
        float bestScore = float.NegativeInfinity;
        var candidates = hoveredInteractables.ToList();

        for (int i = 0; i < candidates.Count; i++)
        {
            var it = candidates[i];
            if (it == null) continue;

            var itMB = it as MonoBehaviour;
            if (itMB == null) continue;

            float dist = Vector3.Distance(transform.position, itMB.transform.position);
            float score = -dist;
            if (score > bestScore)
            {
                bestScore = score;
                best = it;
            }
        }

        SetCandidate(best);
    }

    public void ForceSelection(IInteractable interactable)
    {
        if (interactable == null)
            return;

        StopInteraction();
        SetCandidate(interactable);
        StartInteraction(interactable);
    }

    private void SetCandidate(IInteractable next)
    {
        if (ReferenceEquals(Candidate, next))
            return;

        Candidate?.OnHoverEnd(this);

        Candidate = next;

        Candidate?.OnHoverStart(this);
    }


    private void Interact(bool active)
    {
        if (active && _canInteract && Selected == null && Candidate != null)
        {
            StartInteraction(Candidate);
        }
        else if (Selected != null)
        {
            StopInteraction();
        }
    }

    private void StartInteraction(IInteractable target)
    {
        Selected = target;
        _canInteract = false;

        Selected.OnInteractStart(this);
        hand?.CloseHand(true);
        interactableColliders.Clear();
        Selected.CollectInteractionColliders(interactableColliders);
        //interactableColliders.Add(handTransform.GetComponent<Collider>());
        IgnoreCollisionWithInteractable(true);
    }

    private void StopInteraction()
    {
        if (Selected == null)
            return;

        Selected.OnInteractEnd(this);
        IgnoreCollisionWithInteractable(false);
        interactableColliders.Clear();
        Selected = null;
        hand?.CloseHand(false);
        StartCoroutine(Cooldown());
        RefreshCandidate();
    }

    private void IgnoreCollisionWithInteractable(bool toggle)
    {
        foreach (var handCollider in handColliders)
        {
            foreach (var interactableCollider in interactableColliders)
            {
                Physics.IgnoreCollision(handCollider, interactableCollider, toggle);
            }
        }
    }

    private void FixedUpdate()
    {
        FollowHand();
        UpdateVelocity(Time.fixedDeltaTime);
    }

    private void FollowHand()
    {
        if (handTransform == null) return;

        Vector3 worldPosition = handTransform.TransformPoint(offset);
        transform.SetPositionAndRotation(worldPosition, handTransform.rotation);
    }

    private void UpdateVelocity(float dt)
    {
        if (dt > Mathf.Epsilon)
        {
            CurrentVelocity = (transform.position - _lastPosition) / dt;
        }
        else
        {
            CurrentVelocity = Vector3.zero;
        }

        _lastPosition = transform.position;
    }

    private IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(cooldown);
        _canInteract = true;

    }
}
