using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using VContainer;

public class Interactor : MonoBehaviour
{

    public Vector3 CurrentVelocity { get; private set; }

    [SerializeField] Transform handTransform;
    [SerializeField] Vector3 offset;
    [field: SerializeField] public Transform SnapPoint { get; private set; }

    [SerializeField] float cooldown = 0.5f;
    bool _canInteract = true;
    HandFollower hand;
    Vector3 _lastPosition;

    List<Collider> handColliders = null;
    List<Collider> interactableColliders = new();


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
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent<IInteractable>(out var interactable))
        {
            hoveredInteractables.Add(interactable);
            RefreshCandidate();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.TryGetComponent<IInteractable>(out var interactable))
        {
            hoveredInteractables.Remove(interactable);
            RefreshCandidate();
        }
    }

    private void RefreshCandidate()
    {
        if (Selected != null)
            return;

        IInteractable best = null;
        float bestScore = float.NegativeInfinity;
        List<Interactable> candidates = new List<Interactable>();
        candidates.AddRange(hoveredInteractables);

        for (int i = 0; i < candidates.Count; i++)
        {
            var it = candidates[i];
            if (it == null) continue;

            float dist = Vector3.Distance(transform.position, it.transform.position);
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

        Candidate?.HideOutline();

        Candidate = next;

        Candidate?.ShowOutline();
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
        Selected.Interact(this);
        hand?.CloseHand(true);
        interactableColliders = transform.GetComponentsInChildren<Collider>().Skip(1).ToList();
        IgnoreCollisionWithInteractable(true);
    }

    private void StopInteraction()
    {
        Selected.StopInteract(this);
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
            //Debug.Log($"Hand Colliders: {handCollider.name}");
            foreach (var interactableCollider in interactableColliders)
            {
                //Debug.Log($"Interactable Colliders: {interactableCollider.name}");
                Physics.IgnoreCollision(handCollider, interactableCollider, toggle);
            }
        }
    }

    private void FixedUpdate()
    {
        CurrentVelocity = (transform.position - _lastPosition) / Time.fixedDeltaTime;
        _lastPosition = transform.position;
    }

    private void LateUpdate()
    {
        FollowHand();
    }

    private void FollowHand()
    {
        if (handTransform == null) return;
        transform.position = handTransform.TransformPoint(offset);
        transform.rotation = handTransform.rotation;
    }

    private IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(cooldown);
        _canInteract = true;

    }
}
