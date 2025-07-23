using System;
using System.Collections;
using UnityEngine;

public class Interactor : MonoBehaviour
{
    [SerializeField] private Transform snapPoint;
    [SerializeField] private float cooldown = 0.5f;

    public IInteractable OverlapedInteractable { get; set; }
    private IInteractable _interactable;
    private bool _canInteract = true;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent<IInteractable>(out var interactable))
        {
            OverlapedInteractable = interactable;
            interactable.ShowOutline();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.TryGetComponent<IInteractable>(out var interactable))
        {
            OverlapedInteractable = null;
            interactable.HideOutline();
        }
    }

    private void Update()
    {
        if(_interactable == null) _canInteract = true;

        if (Input.GetKeyDown(KeyCode.E) && OverlapedInteractable != null && _canInteract)
        {
            _interactable = OverlapedInteractable.Use(snapPoint);
            _canInteract = false;
        }
        if (Input.GetKeyUp(KeyCode.E) && _interactable != null)
        {
            _interactable.Release();
            _interactable = null;   
            StartCoroutine(Cooldown());
        }
    }

    private IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(cooldown);
        _canInteract = true;

    }
}
