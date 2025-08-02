using System.Collections;
using UnityEngine;
using UnityEngine.XR;

public class Interactor : MonoBehaviour
{
    [SerializeField] private Transform handTransform;
    [SerializeField] private Vector3 offset;
    [field: SerializeField] public Transform SnapPoint { get; private set; }
    [field: SerializeField] public Interactable OverlapedInteractable { get; set; }

    [SerializeField] private float cooldown = 0.5f;
    private Interactable _interactable;
    private bool _canInteract = true;
    private HandFollower hand;

    Collider[] handColliders = null;
    Collider[] interactableColliders = null;

    private void OnValidate()
    {
        FollowHand();
    }

    private void Start()
    {
        handColliders = handTransform.GetComponentsInChildren<Collider>(true);
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

        if ((Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0))&& OverlapedInteractable != null && _canInteract)
        {
            OverlapedInteractable.Interact(this);
            _canInteract = false;
            _interactable = OverlapedInteractable;
            hand.CloseHand(true);
            interactableColliders = _interactable.GetComponentsInChildren<Collider>();
            IngoreCollisionWithInteractable(true);
        }
        if ((Input.GetKeyUp(KeyCode.E) || Input.GetMouseButtonUp(0)) && _interactable != null)
        {
            _interactable.StopInteract(this);
            IngoreCollisionWithInteractable(false);
            _interactable = null;
            hand.CloseHand(false);
            StartCoroutine(Cooldown());
        }
    }


    private void IngoreCollisionWithInteractable(bool toggle)
    {
        foreach (var handCollider in handColliders)
        {
            Debug.Log($"Hand Colliders: {handCollider.name}");
            foreach (var interactableCollider in interactableColliders)
            {
                Debug.Log($"Interactable Colliders: {interactableCollider.name}");
                Physics.IgnoreCollision(handCollider, interactableCollider, toggle);
            }
        }
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
