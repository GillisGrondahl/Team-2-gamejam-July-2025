using UnityEngine;
using MoreMountains.Feedbacks;
using LineworkLite.Common.Attributes;

[RequireComponent(typeof(Rigidbody))]
public class Interactable : MonoBehaviour
{
    private Transform _target;
    private Rigidbody _rigidbody;
    private Material outlineMaterial;
    public Ingredient ingredient;
    public LayerMask defaultLayer;
    public LayerMask outlineLayer;

    private bool _resting = true;

    [Header("MM Feedbacks")]
    [SerializeField] private MMF_Player _fdbkPickUp;
    [SerializeField] private MMF_Player _fdbkDropped;
    [Tooltip("Minimum velocity to trigger drop feedback")] [SerializeField] private float _velocityThreshold = 0.5f;


    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        //outlineMaterial = GetComponent<Renderer>().materials[1];
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

    public void SnapTo(Transform target)
    {
        _resting = false;
        _target = target;
        _rigidbody.isKinematic = true;
        //HideOutline();

       

        if (_fdbkPickUp != null)
        {
            _fdbkPickUp.PlayFeedbacks();
        }
        
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
        gameObject.layer = GetLayerFromMask(outlineLayer.value);
        //outlineMaterial.SetFloat("_OutlineThickness", 1.1f);
    }

    public void HideOutline()
    {
        gameObject.layer = GetLayerFromMask(defaultLayer.value);
        //outlineMaterial.SetFloat("_OutlineThickness", 0f);
    }

    int GetLayerFromMask(int mask)
    {
        int layer = 0;
        while ((mask >>= 1) != 0)
            layer++;
        return layer;
    }
}
