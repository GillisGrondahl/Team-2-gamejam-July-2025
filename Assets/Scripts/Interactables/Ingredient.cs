using UnityEngine;
using MoreMountains.Feedbacks;

[RequireComponent(typeof(Rigidbody))]
public class Ingredient : MonoBehaviour
{
    public IngredientData ingredient;
    private Transform _transformToFollow;
    private Rigidbody _rigidbody;
    private Transform _originalParent;

    private bool _resting = true;

    [Header("MM Feedbacks")]
    [SerializeField] private MMF_Player _fdbkPickUp;
    [SerializeField] private MMF_Player _fdbkDropped;

    [Tooltip("Minimum velocity to trigger drop feedback")] 
    [SerializeField] private float _velocityThreshold = 0.5f;


    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _originalParent = transform.parent;
    }

    private void LateUpdate()
    {
        FollowTarget();
    }

    private void FollowTarget()
    {
        if (_transformToFollow == null) return;

        transform.position = _transformToFollow.position;
        transform.rotation = _transformToFollow.rotation;
    }

    public void PickUp(Interactor interactor)
    {
        interactor.OverlapedInteractable = GetComponent<Interactable>();
        _resting = false;
        _transformToFollow = interactor.SnapPoint != null ? interactor.SnapPoint : interactor.transform;
        _rigidbody.isKinematic = true;
        transform.SetParent(interactor.transform);

        if (_fdbkPickUp != null)
        {
            _fdbkPickUp.PlayFeedbacks();
        }        
    }
    public void Release(Interactor interactor)
    {
        _transformToFollow = null;
        _rigidbody.isKinematic = false;
        transform.SetParent(_originalParent.transform);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Check if we're not being held, hit something other than the player, and have enough y-velocity
        if (collision.gameObject.tag != "Player" && _transformToFollow == null && _resting == false && _rigidbody.linearVelocity.y <= _velocityThreshold)
        {
            _resting = true;

            if (_fdbkDropped != null)
            {
                _fdbkDropped.PlayFeedbacks();
            }
        }
    }
}
