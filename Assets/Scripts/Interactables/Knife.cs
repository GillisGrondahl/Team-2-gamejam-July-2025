using System.Collections;
using UnityEngine;

public class Knife : MonoBehaviour
{
    Transform _originalParent;

    private Rigidbody _rigidbody;
    private Collider _collider;

    private bool _isKnifing = false;
    private Ingredient _cutIngredient = null;


    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
        _originalParent = transform.parent;

    }
    public void PickUp(Interactor interactor)
    {
        _rigidbody.isKinematic = true;
        _collider.isTrigger = true;
        transform.SetParent(interactor.transform);
    }
    public void Release(Interactor interactor)
    {
        _rigidbody.isKinematic = false;
        _collider.isTrigger = false;
        transform.SetParent(_originalParent.transform);
    }
}

