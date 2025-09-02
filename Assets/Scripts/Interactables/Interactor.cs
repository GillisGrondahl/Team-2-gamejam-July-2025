using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Interactor : MonoBehaviour
{

    public Vector3 CurrentVelocity { get; private set; }

    [SerializeField] private Transform handTransform;
    [SerializeField] private Vector3 offset;
    [field: SerializeField] public Transform SnapPoint { get; private set; }
    [field: SerializeField] public Interactable OverlapedInteractable { get; set; }

    [SerializeField] private float cooldown = 0.5f;
    private Interactable _interactable;
    private bool _canInteract = true;
    private HandFollower hand;
    private Vector3 _lastPosition;

    List<Collider> handColliders = null;
    List<Collider> interactableColliders = new();

    private void OnValidate()
    {
        FollowHand();
    }

    private void Start()
    {
        handColliders = handTransform.GetComponentsInChildren<Collider>(true).ToList();
        hand = handTransform.GetComponent<HandFollower>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent<Interactable>(out var interactable))
        {
            OverlapedInteractable = interactable;
            interactable.ShowOutline();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.TryGetComponent<Interactable>(out var interactable))
        {
            OverlapedInteractable = null;
            interactable.HideOutline();
        }
    }


    private void Update()
    {
        if (_interactable == null) _canInteract = true;

        if ((Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0)) && OverlapedInteractable != null && _canInteract)
        {
            OverlapedInteractable.Interact(this);
            _canInteract = false;
            _interactable = OverlapedInteractable;
            hand.CloseHand(true);

            interactableColliders = transform.GetComponentsInChildren<Collider>().Skip(1).ToList();
            //GetColliders();
            IngoreCollisionWithInteractable(true);
        }
        if ((Input.GetKeyUp(KeyCode.E) || Input.GetMouseButtonUp(0)) && _interactable != null)
        {
            _interactable.StopInteract(this);
            IngoreCollisionWithInteractable(false);
            interactableColliders.Clear();
            _interactable = null;
            hand.CloseHand(false);
            StartCoroutine(Cooldown());
        }
    }

    //private void GetColliders()
    //{
    //    //var interactableParent = _interactable.transform.parent;
    //    //if (interactableParent != null && interactableParent.TryGetComponent<Interactable>(out var interactableComponent))
    //    //{
    //    //    _interactable = interactableComponent;
    //    //}

    //    //interactableColliders = _interactable.GetComponentsInChildren<Collider>().ToList();
    //    interactableColliders = transform.GetComponentsInChildren<Collider>().Skip(1).ToList();
    //}


    private void IngoreCollisionWithInteractable(bool toggle)
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
