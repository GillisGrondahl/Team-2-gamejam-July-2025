using MoreMountains.Feedbacks;
using System.Collections;
using UnityEngine;

public class Tool : MonoBehaviour
{
    protected Transform _originalParent;
    protected Rigidbody _rigidbody;
    protected Collider _collider;
    protected Vector3 _startingPosition;

    private bool _resting = true;

    [Tooltip("Minimum velocity to trigger drop feedback")]
    [SerializeField] private float _velocityThreshold = 0.5f;

    [Header("MM Feedbacks")]
    [SerializeField] private MMF_Player _fdbkPickUp;
    [SerializeField] private MMF_Player _fdbkDropped;
    [SerializeField] private MMF_Player _fbdkToolReset;

    protected virtual void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
        _originalParent = transform.parent;
        _startingPosition = transform.localPosition;
    }

    public void PickUp(Interactor interactor)
    {
        _rigidbody.isKinematic = true;
        _collider.isTrigger = true;
        transform.SetParent(interactor.transform);

        _resting = false;


        if (_fdbkPickUp != null && !_fdbkPickUp.IsPlaying && !_fdbkDropped.IsPlaying)
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

            if (_fdbkDropped != null && !_fdbkPickUp.IsPlaying && !_fdbkDropped.IsPlaying)
            {
                _fdbkDropped.PlayFeedbacks();
            }
        }
    }

    public void ResetPosition()
    {
        
        transform.parent = _originalParent;
        transform.localPosition = _startingPosition;
        _resting = true;

        _fbdkToolReset.PlayFeedbacks();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.R) && _resting)
        {
            ResetPosition();
        }
    }

    //private void OnTriggerEnter(Collider trigger)
    //{
    //    if (trigger.gameObject.TryGetComponent<IngredientChecker>(out var ingredientChecker))
    //    {
    //        Debug.Log("Tool dropped in pot");

    //        transform.position = _startingPosition;

    //        if (_fbdkDroppedInPot != null)
    //        {
    //            _fbdkDroppedInPot.PlayFeedbacks();
    //        }

    //        if (_fdbkDropped != null)
    //        {
    //            _fdbkDropped.PlayFeedbacks();
    //        }

    //        _resting = true;

    //    }


    //}

}

