using UnityEngine;
using MoreMountains.Feedbacks;

[RequireComponent(typeof(Rigidbody))]
public class Ingredient : MonoBehaviour
{
    public IngredientData ingredient = null;
    public GameObject splashPrefab = null;
    public Ingredient ParentIngredient = null;
    public Transform TransformToFollow = null;
    public Rigidbody Rigidbody = null;
    private Transform _originalParent = null;
    public bool IsAPart = false;

    private bool _resting = true;

    [Header("MM Feedbacks")]
    [SerializeField] private MMF_Player _fdbkPickUp;
    [SerializeField] private MMF_Player _fdbkDropped;

    [Tooltip("Minimum velocity to trigger drop feedback")]
    [SerializeField] private float _velocityThreshold = 0.5f;

    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody>();
        if (transform.parent != null)
        {
            _originalParent = transform.parent;
            ParentIngredient = transform.parent.GetComponent<Ingredient>();
        }
    }

    private void Start()
    {
        if (Rigidbody == null)
            Rigidbody = GetComponent<Rigidbody>();
        if (_originalParent == null)
            _originalParent = transform.parent;
        if (ParentIngredient == null)
            ParentIngredient = transform.parent.GetComponent<Ingredient>();
    }

    private void LateUpdate()
    {
        FollowTarget();
    }

    private void FollowTarget()
    {
        if (TransformToFollow == null) return;

        transform.position = TransformToFollow.position;
    }

    public void PickUp(Interactor interactor)
    {
        if (IsAPart)
        {
            ParentIngredient.PickUp(interactor);
            return;
        }

        interactor.OverlapedInteractable = GetComponent<Interactable>();

        foreach (var ingredient in GetComponentsInChildren<Ingredient>())
        {
            ingredient.TransformToFollow = interactor.SnapPoint != null ? interactor.SnapPoint : interactor.transform;
            ingredient.Rigidbody.isKinematic = true;
        }
        _resting = false;

        transform.SetParent(interactor.transform);

        if (_fdbkPickUp != null)
        {
            _fdbkPickUp.PlayFeedbacks();
        }
    }
    public void Release(Interactor interactor)
    {
        if (IsAPart)
        {
            ParentIngredient.Release(interactor);
            return;
        }

        foreach (var ingredient in GetComponentsInChildren<Ingredient>())
        {
            ingredient.TransformToFollow = null;
            ingredient.Rigidbody.isKinematic = false;
        }

        //TransformToFollow = null;
        //Rigidbody.isKinematic = false;
        transform.SetParent(_originalParent.transform);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Check if we're not being held, hit something other than the player, and have enough y-velocity
        if (collision.gameObject.tag != "Player" && TransformToFollow == null && _resting == false && Rigidbody.linearVelocity.y <= _velocityThreshold)
        {
            _resting = true;

            if (_fdbkDropped != null)
            {
                _fdbkDropped.PlayFeedbacks();
            }
        }

        if(collision.gameObject.tag == "Table")
        {
            var table = collision.gameObject;

            if (splashPrefab != null && Rigidbody.linearVelocity.magnitude > _velocityThreshold)
            {
                var contact = collision.contacts[0];
                var splash = Instantiate(splashPrefab, contact.point + contact.normal * 0.01f, Quaternion.LookRotation(-contact.normal), table.transform);
            }
        }
    }
}
