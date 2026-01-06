using UnityEngine;
using MoreMountains.Feedbacks;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class Ingredient : MonoBehaviour
{
    public IngredientData ingredient = null;
    public GameObject splashPrefab = null;
    public Ingredient ParentIngredient = null;
    public Transform TransformToFollow = null;
    public Rigidbody Rigidbody = null;
    public bool cutable = true; // Change to property
    private Transform _originalParent = null;

    public List<Ingredient> ingredientParts = new List<Ingredient>();

    //Move MMF to it's own feedback class
    private bool _resting = true;

    [Header("MM Feedbacks")]
    [SerializeField] private MMF_Player _fdbkPickUp;
    [SerializeField] private MMF_Player _fdbkDropped;

    [Tooltip("Minimum velocity to trigger drop feedback")]
    [SerializeField] private float _velocityThreshold = 0.5f;

    private void Awake()
    {
        TryGetComponent(out Rigidbody);
        if (transform.parent != null)
        {
            _originalParent = transform.parent;
        }
    }

    private void Start()
    {
        if (Rigidbody == null)
        {
            TryGetComponent(out Rigidbody);
            Rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }
        if (_originalParent == null)
            _originalParent = transform.parent;
    }

    private void Update()
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
        if (ParentIngredient != null)
        {
            ParentIngredient.PickUp(interactor);
            return;
        }

        TransformToFollow = interactor.SnapPoint != null ? interactor.SnapPoint : interactor.transform;
        Rigidbody.isKinematic = true;
        transform.SetParent(interactor.transform);

        foreach (var ingredient in ingredientParts)
        {
            ingredient.TransformToFollow = interactor.SnapPoint != null ? interactor.SnapPoint : interactor.transform;
            ingredient.Rigidbody.isKinematic = true;
            ingredient.transform.SetParent(interactor.transform);
        }
        _resting = false;


        if (_fdbkPickUp != null && !_fdbkPickUp.IsPlaying && !_fdbkDropped.IsPlaying)
        {
            _fdbkPickUp.PlayFeedbacks();
        }
    }

    public void Release(Interactor interactor)
    {
        if (ParentIngredient != null)
        {
            ParentIngredient.Release(interactor);
            return;
        }

        TransformToFollow = null;
        Rigidbody.isKinematic = false;
        transform.SetParent(_originalParent.transform);

        foreach (var ingredient in ingredientParts)
        {
            ingredient.TransformToFollow = null;
            ingredient.Rigidbody.isKinematic = false;
            ingredient.transform.SetParent(_originalParent.transform);
        }

    }

    private void OnCollisionEnter(Collision collision)
    {
        // Check if we're not being held, hit something other than the player, and have enough y-velocity
        if (collision.gameObject.tag != "Player" && TransformToFollow == null && _resting == false && Rigidbody.linearVelocity.y <= _velocityThreshold)
        {
            _resting = true;

            if (_fdbkDropped != null && !_fdbkPickUp.IsPlaying && !_fdbkDropped.IsPlaying)
            {
                _fdbkDropped.PlayFeedbacks();
            }
        }

        if (collision.gameObject.tag == "Table")
        {
            var table = collision.gameObject;

            if (splashPrefab != null && Rigidbody.linearVelocity.magnitude > _velocityThreshold)
            {
                var contact = collision.contacts[0];
                var splash = Instantiate(splashPrefab, contact.point + contact.normal * 0.01f, Quaternion.LookRotation(-contact.normal), table.transform);
            }
        }
    }

    private void OnDestroy()
    {
        foreach (var ingredient in ingredientParts)
            Destroy(ingredient.gameObject);
    }
}
