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
    private const float MinDistanceEpsilon = 0.0001f;
    private const string ReasonFree = "NoContact";
    private const string ReasonPrimaryHit = "PrimarySweepHit";
    private const string ReasonPrimaryBlocked = "PrimaryBlocked";
    private const string ReasonSlideResolved = "SlideResolved";
    private const string ReasonSlideBlocked = "SlideBlocked";
    private const string ReasonDepenetrationGuard = "DepenetrationGuard";

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
    private bool _missingCapsuleLogged;

    private Rigidbody _rb;
    private CapsuleCollider _capsuleCollider;
    private ArmContactState _currentState;
    private string _lastTransitionReason = ReasonFree;
    private Vector3 _lastContactNormal;
    private Vector3 _linearVelocity;
    private int _stuckFrameCount;
    private HandController _handController;

    private readonly RaycastHit[] _castHits = new RaycastHit[8];
    private readonly Collider[] _overlapBuffer = new Collider[16];

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        ConfigureRigidbody();

        if (handCollider == null)
            handCollider = GetComponent<Collider>();

        if (handCollider == null)
            handCollider = GetComponentInChildren<Collider>(true);

        _capsuleCollider = handCollider as CapsuleCollider;

        if (excludeSelfLayerFromCollisionMask)
            collisionMask &= ~(1 << gameObject.layer);
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

        Vector3 resolvedPosition = ResolveCollisionCastAndSlide(
            currentPosition,
            desiredPosition,
            desiredRotation,
            out ArmContactState nextState,
            out Vector3 contactNormal,
            out string transitionReason);

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

    private Vector3 ResolveCollisionCastAndSlide(
        Vector3 currentPosition,
        Vector3 desiredPosition,
        Quaternion desiredRotation,
        out ArmContactState state,
        out Vector3 contactNormal,
        out string reason)
    {
        state = ArmContactState.Free;
        contactNormal = Vector3.zero;
        reason = ReasonFree;

        if (handCollider == null || !handCollider.enabled || handCollider.isTrigger)
            return desiredPosition;

        if (_capsuleCollider == null || _capsuleCollider.transform != transform)
        {
            if (!_missingCapsuleLogged)
            {
                Debug.LogWarning($"{nameof(HandFollower)} on '{name}' requires a CapsuleCollider on the same object for cast-and-slide.", this);
                _missingCapsuleLogged = true;
            }

            return ApplyDepenetrationGuard(desiredPosition, desiredRotation, ref state, ref contactNormal, ref reason);
        }

        _missingCapsuleLogged = false;

        Vector3 displacement = desiredPosition - currentPosition;
        float distance = displacement.magnitude;

        if (distance <= MinDistanceEpsilon)
            return ApplyDepenetrationGuard(desiredPosition, desiredRotation, ref state, ref contactNormal, ref reason);

        Vector3 direction = displacement / distance;
        Vector3 resolvedPosition = desiredPosition;

        if (TryCapsuleCast(currentPosition, desiredRotation, direction, distance + skin, out RaycastHit firstHit))
        {
            float firstMove = Mathf.Max(0f, firstHit.distance - skin);
            Vector3 firstStep = currentPosition + direction * firstMove;

            state = ArmContactState.Contact;
            contactNormal = firstHit.normal;
            reason = ReasonPrimaryHit;

            Vector3 consumed = direction * firstMove;
            Vector3 remaining = displacement - consumed;
            Vector3 slide = Vector3.ProjectOnPlane(remaining, firstHit.normal) * Mathf.Clamp01(collisionSlideFactor);

            float slideDistance = slide.magnitude;
            if (slideDistance > MinDistanceEpsilon)
            {
                Vector3 slideDir = slide / slideDistance;

                if (TryCapsuleCast(firstStep, desiredRotation, slideDir, slideDistance + skin, out RaycastHit slideHit))
                {
                    float secondMove = Mathf.Max(0f, slideHit.distance - skin);
                    resolvedPosition = firstStep + slideDir * secondMove;
                    contactNormal = slideHit.normal;
                    reason = ReasonSlideBlocked;

                    if (firstMove <= MinDistanceEpsilon && secondMove <= MinDistanceEpsilon)
                        state = ArmContactState.Blocked;
                }
                else
                {
                    resolvedPosition = firstStep + slide;
                    reason = ReasonSlideResolved;
                }
            }
            else
            {
                resolvedPosition = firstStep;
                if (firstMove <= MinDistanceEpsilon)
                {
                    state = ArmContactState.Blocked;
                    reason = ReasonPrimaryBlocked;
                }
            }
        }

        return ApplyDepenetrationGuard(resolvedPosition, desiredRotation, ref state, ref contactNormal, ref reason);
    }

    private Vector3 ApplyDepenetrationGuard(
        Vector3 desiredPosition,
        Quaternion desiredRotation,
        ref ArmContactState state,
        ref Vector3 contactNormal,
        ref string reason)
    {
        if (_capsuleCollider == null)
            return desiredPosition;

        Vector3 corrected = desiredPosition;

        for (int iteration = 0; iteration < depenetrationIterations; iteration++)
        {
            if (!GetCapsuleGeometry(corrected, desiredRotation, out Vector3 pointA, out Vector3 pointB, out float radius))
                break;

            int overlapCount = Physics.OverlapCapsuleNonAlloc(
                pointA,
                pointB,
                radius,
                _overlapBuffer,
                collisionMask,
                QueryTriggerInteraction.Ignore);

            bool correctedThisIteration = false;

            for (int i = 0; i < overlapCount; i++)
            {
                Collider other = _overlapBuffer[i];
                if (!IsValidCollisionCandidate(other))
                    continue;

                if (!Physics.ComputePenetration(
                        handCollider,
                        corrected,
                        desiredRotation,
                        other,
                        other.transform.position,
                        other.transform.rotation,
                        out Vector3 depenetrationDirection,
                        out float depenetrationDistance))
                {
                    continue;
                }

                if (depenetrationDistance <= 0f)
                    continue;

                corrected += depenetrationDirection * (depenetrationDistance + skin);
                contactNormal = depenetrationDirection;
                correctedThisIteration = true;

                if (state == ArmContactState.Free)
                    state = ArmContactState.Contact;

                reason = ReasonDepenetrationGuard;
            }

            if (!correctedThisIteration)
                break;
        }

        return corrected;
    }

    private bool TryCapsuleCast(
        Vector3 fromPosition,
        Quaternion castRotation,
        Vector3 direction,
        float distance,
        out RaycastHit bestHit)
    {
        bestHit = default;

        if (distance <= MinDistanceEpsilon)
            return false;

        if (!GetCapsuleGeometry(fromPosition, castRotation, out Vector3 pointA, out Vector3 pointB, out float radius))
            return false;

        int hitCount = Physics.CapsuleCastNonAlloc(
            pointA,
            pointB,
            radius,
            direction,
            _castHits,
            distance,
            collisionMask,
            QueryTriggerInteraction.Ignore);

        bool found = false;
        float bestDistance = float.PositiveInfinity;

        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit hit = _castHits[i];
            if (!IsValidCollisionCandidate(hit.collider))
                continue;

            if (hit.distance < 0f)
                continue;

            if (hit.distance < bestDistance)
            {
                bestDistance = hit.distance;
                bestHit = hit;
                found = true;
            }
        }

        return found;
    }

    private bool GetCapsuleGeometry(
        Vector3 rootPosition,
        Quaternion rootRotation,
        out Vector3 pointA,
        out Vector3 pointB,
        out float radius)
    {
        pointA = rootPosition;
        pointB = rootPosition;
        radius = 0.01f;

        if (_capsuleCollider == null)
            return false;

        Vector3 lossyScale = transform.lossyScale;
        Vector3 absLossy = new Vector3(
            Mathf.Abs(lossyScale.x),
            Mathf.Abs(lossyScale.y),
            Mathf.Abs(lossyScale.z));

        Vector3 axis;
        float radiusScale;
        float heightScale;

        switch (_capsuleCollider.direction)
        {
            case 0:
                axis = Vector3.right;
                radiusScale = Mathf.Max(absLossy.y, absLossy.z);
                heightScale = absLossy.x;
                break;
            case 1:
                axis = Vector3.up;
                radiusScale = Mathf.Max(absLossy.x, absLossy.z);
                heightScale = absLossy.y;
                break;
            default:
                axis = Vector3.forward;
                radiusScale = Mathf.Max(absLossy.x, absLossy.y);
                heightScale = absLossy.z;
                break;
        }

        radius = Mathf.Max(0.0001f, _capsuleCollider.radius * radiusScale);
        float height = Mathf.Max(_capsuleCollider.height * heightScale, radius * 2f);
        float halfSegment = Mathf.Max(0f, height * 0.5f - radius);

        Vector3 scaledCenter = Vector3.Scale(_capsuleCollider.center, lossyScale);
        Vector3 worldCenter = rootPosition + rootRotation * scaledCenter;
        Vector3 worldAxis = rootRotation * axis;

        pointA = worldCenter + worldAxis * halfSegment;
        pointB = worldCenter - worldAxis * halfSegment;

        return true;
    }

    private bool IsValidCollisionCandidate(Collider other)
    {
        if (other == null)
            return false;

        if (!other.enabled || other.isTrigger)
            return false;

        if (other == handCollider)
            return false;

        if (other.attachedRigidbody == _rb)
            return false;

        if (handCollider != null && Physics.GetIgnoreCollision(handCollider, other))
            return false;

        return true;
    }

    private void ConfigureRigidbody()
    {
        _rb.isKinematic = true;
        _rb.useGravity = false;
        _rb.detectCollisions = true;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
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
