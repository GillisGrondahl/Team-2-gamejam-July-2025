using UnityEngine;
using MoreMountains.Feedbacks;

[RequireComponent(typeof(Rigidbody))]
public class Ingredient : MonoBehaviour, IInteractable
{
    public IngredientData ingredient;
    [field: SerializeField] public LayerMask OutlineLayer { get; private set; }
    public LayerMask OriginalLayer { get; private set; }

    private Transform _target;
    private Rigidbody _rigidbody;

    private bool _resting = true;

    [Header("MM Feedbacks")]
    [SerializeField] private MMF_Player _fdbkPickUp;
    [SerializeField] private MMF_Player _fdbkDropped;
    [Tooltip("Minimum velocity to trigger drop feedback")] 
    [SerializeField] private float _velocityThreshold = 0.5f;


    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        OriginalLayer = gameObject.layer;
    }

    private void LateUpdate()
    {
        FollowTarget();
    }

    private void FollowTarget()
    {
        if (_target == null) return;

        transform.position = _target.position;
        transform.rotation = _target.rotation;
    }

    public IInteractable Use(Transform target)
    {
        _resting = false;
        _target = target;
        _rigidbody.isKinematic = true;          

        if (_fdbkPickUp != null)
        {
            _fdbkPickUp.PlayFeedbacks();
        }

        return this;        
    }
    public void Release()
    {
        _target = null;
        _rigidbody.isKinematic = false;
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Check if we're not being held, hit something other than the player, and have enough y-velocity
        if (collision.gameObject.tag != "Player" && _target == null && _resting == false && _rigidbody.linearVelocity.y <= _velocityThreshold)
        {
            _resting = true;

            if (_fdbkDropped != null)
            {
                _fdbkDropped.PlayFeedbacks();
            }
        }
    }

    public void ShowOutline()
    {
        gameObject.layer = ((IInteractable)this).GetLayerFromMask(OutlineLayer.value);
    }

    public void HideOutline()
    {
        gameObject.layer = ((IInteractable)this).GetLayerFromMask(OriginalLayer.value);
    }
}
