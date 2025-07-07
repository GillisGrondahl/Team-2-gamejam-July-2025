using System;
using System.Collections;
using UnityEngine;

public class Interactor : MonoBehaviour
{
    [SerializeField] private Transform snapPoint;
    [SerializeField] private float cooldown = 0.5f;

    public Interactable OverlapedInteractable { get; set; }
    private Interactable _interactable;
    private bool _canInteract = true;

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
        if (Input.GetKeyDown(KeyCode.E) && OverlapedInteractable != null)
        {
            _interactable = OverlapedInteractable;
            _interactable.SnapTo(snapPoint);
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
