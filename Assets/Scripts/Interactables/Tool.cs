using MoreMountains.Feedbacks;
using System.Collections;
using UnityEngine;

public class Tool : MonoBehaviour
{
    private Transform _originalParent;
    private Rigidbody _rigidbody;
    private Collider _collider;

    private bool _resting = true;

    [Tooltip("Minimum velocity to trigger drop feedback")]
    [SerializeField] private float _velocityThreshold = 0.5f;

    [Header("MM Feedbacks")]
    [SerializeField] private MMF_Player _fdbkPickUp;
    [SerializeField] private MMF_Player _fdbkDropped;

    protected virtual void Awake()
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

        _resting = false;


        if (_fdbkPickUp != null)
        {
            _fdbkPickUp.PlayFeedbacks();
        }
    }
    public void Release(Interactor interactor)
    {
        _rigidbody.isKinematic = false;
        _collider.isTrigger = false;
        transform.SetParent(_originalParent.transform);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Check if we're resting, and have enough y-velocity
        if (collision.gameObject.tag != "Player" && _resting == false && _rigidbody.linearVelocity.y <= _velocityThreshold)
        {
            _resting = true;

            if (_fdbkDropped != null)
            {
                _fdbkDropped.PlayFeedbacks();
            }
        }

 
    }

}

