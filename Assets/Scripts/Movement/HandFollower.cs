using UnityEngine;

public enum ArmContactState
{
    Free,
    Contact,
    Blocked
}

[DefaultExecutionOrder(-100)]
[RequireComponent(typeof(Rigidbody))]
public sealed class HandFollower : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Follow")]
    [SerializeField] private float positionStiffness = 30f;
    [SerializeField] private float rotationStiffness = 10f;
    [SerializeField] private float maxLinearSpeed = 10f;
    [SerializeField] private float maxLinearAcceleration = 80f;
    [SerializeField] private float maxAngularSpeed = 20f;
    [SerializeField] private float reachLinearSpeedMultiplier = 6f;
    [SerializeField] private float reachLinearAccelerationMultiplier = 8f;

    [Header("Collision")]
    [Tooltip("The collider representing the hand collision volume (NOT a trigger).")]
    [SerializeField] private Collider handCollider;

    [Tooltip("Which layers the hand should collide against.")]
    [SerializeField] private LayerMask collisionMask = ~0;

    [Tooltip("Exclude the hand object's own layer from collision mask to avoid self/player blocking.")]
    [SerializeField] private bool excludeSelfLayerFromCollisionMask = true;

    [Tooltip("How much of the remaining movement is preserved while sliding.")]
    [Range(0f, 1f)]
    [SerializeField] private float collisionSlideFactor = 0.9f;

    [Tooltip("How many depenetration guard iterations to run per FixedUpdate.")]
    [Range(1, 8)]
    [SerializeField] private int depenetrationIterations = 2;

    [Tooltip("Small extra offset to prevent immediate re-penetration due to float precision.")]
    [SerializeField] private float skin = 0.002f;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    [Header("Debug")]
    [SerializeField] private bool debugDraw;
    [SerializeField] private bool logStateTransitions;
    [SerializeField] private bool enableStuckRecovery = true;
    [SerializeField] private float stuckRecoveryDistance = 0.12f;
    [SerializeField] private int stuckRecoveryFrames = 8;

    public ArmContactState CurrentState => _currentState;
    public string LastTransitionReason => _lastTransitionReason;
    public Vector3 LastContactNormal => _lastContactNormal;

    private bool _isClosing;
    private bool _missingTargetLogged;

    private Rigidbody _rb;
    private ArmContactState _currentState;
    private string _lastTransitionReason = "NoContact";
    private Vector3 _lastContactNormal;
    private Vector3 _linearVelocity;
    private int _stuckFrameCount;
    private HandController _handController;
    private HandCollisionResolver _collisionResolver;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        ConfigureRigidbody();

        if (handCollider == null)
            handCollider = GetComponent<Collider>();

        if (handCollider == null)
            handCollider = GetComponentInChildren<Collider>(true);

        if (excludeSelfLayerFromCollisionMask)
            collisionMask &= ~(1 << gameObject.layer);

        RebuildCollisionResolver();
    }

    private void Start()
    {
        TryResolveTarget();

        if (animator != null)
        {
            animator.Play("HandClose", 0, 0f);
            animator.speed = 0f;
        }
    }

    private void FixedUpdate()
    {
        if (!TryResolveTarget())
        {
            if (!_missingTargetLogged)
            {
                Debug.LogWarning($"{nameof(HandFollower)} on '{name}' has no target set. Assign the hand point transform.", this);
                _missingTargetLogged = true;
            }

            _linearVelocity = Vector3.zero;
            return;
        }

        _missingTargetLogged = false;

        Vector3 currentPosition = _rb.position;
        Quaternion currentRotation = _rb.rotation;
        float dt = Time.fixedDeltaTime;
        bool isReachActive = _handController != null && _handController.IsReachActive;
        Vector3 positionError = target.position - currentPosition;
        Vector3 desiredLinearVelocity = positionError * Mathf.Max(0f, positionStiffness);
        float linearSpeed = Mathf.Max(0f, maxLinearSpeed) * (isReachActive ? Mathf.Max(1f, reachLinearSpeedMultiplier) : 1f);
        float linearAcceleration = Mathf.Max(0f, maxLinearAcceleration) * (isReachActive ? Mathf.Max(1f, reachLinearAccelerationMultiplier) : 1f);
        desiredLinearVelocity = Vector3.ClampMagnitude(desiredLinearVelocity, linearSpeed);
        _linearVelocity = Vector3.MoveTowards(_linearVelocity, desiredLinearVelocity, linearAcceleration * dt);

        Vector3 desiredPosition = currentPosition + _linearVelocity * dt;

        float rotBlend = 1f - Mathf.Exp(-Mathf.Max(0f, rotationStiffness) * dt);
        Quaternion blendedRotation = Quaternion.Slerp(currentRotation, target.rotation, rotBlend);
        float maxRotationStep = Mathf.Max(0f, maxAngularSpeed) * Mathf.Rad2Deg * dt;
        Quaternion desiredRotation = Quaternion.RotateTowards(currentRotation, blendedRotation, maxRotationStep);

        Vector3 resolvedPosition = desiredPosition;
        ArmContactState nextState;
        Vector3 contactNormal;
        string transitionReason;

        if (_collisionResolver != null)
        {
            resolvedPosition = _collisionResolver.Resolve(
                currentPosition,
                desiredPosition,
                desiredRotation,
                out nextState,
                out contactNormal,
                out transitionReason);
        }
        else
        {
            nextState = ArmContactState.Free;
            contactNormal = Vector3.zero;
            transitionReason = "NoContact";
        }

        if (enableStuckRecovery)
        {
            bool noMovementApplied = (resolvedPosition - currentPosition).sqrMagnitude <= 0.0000001f;
            bool targetFarEnough = (target.position - currentPosition).sqrMagnitude >= (stuckRecoveryDistance * stuckRecoveryDistance);

            if (noMovementApplied && targetFarEnough)
            {
                _stuckFrameCount++;
                if (_stuckFrameCount >= Mathf.Max(1, stuckRecoveryFrames))
                {
                    resolvedPosition = desiredPosition;
                    nextState = ArmContactState.Free;
                    transitionReason = "StuckRecoveryBypass";
                    contactNormal = Vector3.zero;
                    _stuckFrameCount = 0;
                }
            }
            else
            {
                _stuckFrameCount = 0;
            }
        }

        _rb.MovePosition(resolvedPosition);
        _rb.MoveRotation(desiredRotation);

        if (dt > Mathf.Epsilon)
            _linearVelocity = (resolvedPosition - currentPosition) / dt;

        UpdateContactState(nextState, transitionReason, contactNormal);

        if (debugDraw)
        {
            Debug.DrawLine(currentPosition, desiredPosition, Color.cyan);
            Debug.DrawLine(currentPosition, resolvedPosition, Color.green);

            if (nextState != ArmContactState.Free)
                Debug.DrawRay(resolvedPosition, contactNormal * 0.25f, Color.red);
        }
    }

    private void RebuildCollisionResolver()
    {
        if (handCollider == null)
        {
            _collisionResolver = null;
            return;
        }

        _collisionResolver = new HandCollisionResolver(
            handCollider,
            _rb,
            transform,
            collisionMask,
            collisionSlideFactor,
            depenetrationIterations,
            skin,
            this);
    }

    private void ConfigureRigidbody()
    {
        _rb.isKinematic = true;
        _rb.useGravity = false;
        _rb.detectCollisions = true;
        _rb.interpolation = RigidbodyInterpolation.None;
        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
    }

    private bool TryResolveTarget()
    {
        if (target != null)
            return true;

        HandController handController = transform.root.GetComponentInChildren<HandController>(true);
        if (handController == null)
            handController = FindFirstObjectByType<HandController>(FindObjectsInactive.Include);

        if (handController != null)
        {
            target = handController.HandPoint;
            _handController = handController;
        }

        return target != null;
    }

    private void UpdateContactState(ArmContactState nextState, string reason, Vector3 contactNormal)
    {
        ArmContactState previousState = _currentState;
        _currentState = nextState;
        _lastTransitionReason = reason;
        _lastContactNormal = contactNormal;

        if (logStateTransitions && previousState != nextState)
        {
            Debug.Log($"{nameof(HandFollower)}: {previousState} -> {nextState} ({reason})", this);
        }
    }

    public void CloseHand(bool close)
    {
        if (animator == null)
        {
            _isClosing = close;
            return;
        }

        if (close && !_isClosing)
        {
            animator.speed = 1.5f;
            animator.Play("HandClose", 0, 0f);
        }
        else if (!close && _isClosing)
        {
            animator.speed = 2.5f;
            animator.Play("HandOpen", 0, 0f);
        }

        _isClosing = close;
    }
}
