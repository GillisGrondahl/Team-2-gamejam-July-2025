using System;
using System.Collections;
using UnityEngine;

public class Interactor : MonoBehaviour
{
    [SerializeField] private Transform handTransform;
    [SerializeField] private Vector3 offset;
    [field: SerializeField] public Transform SnapPoint { get; private set; }
    [field: SerializeField] public Interactable OverlapedInteractable { get; set; }

    [SerializeField] private float cooldown = 0.5f;
    private Interactable _interactable;
    private bool _canInteract = true;

    private void OnValidate()
    {
        if (handTransform == null) return;
        transform.localPosition = handTransform.localPosition + offset;
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

        if (Input.GetKeyDown(KeyCode.E) && OverlapedInteractable != null && _canInteract)
        {
            OverlapedInteractable.Interact(this);
            _canInteract = false;
            _interactable = OverlapedInteractable;
        }
        if (Input.GetKeyUp(KeyCode.E) && _interactable != null)
        {
            _interactable.StopInteract(this);
            _interactable = null;
            StartCoroutine(Cooldown());
        }
    }

    private void FixedUpdate()
    {
        transform.localPosition = handTransform.localPosition + offset;
    }

    private IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(cooldown);
        _canInteract = true;

    }
}
