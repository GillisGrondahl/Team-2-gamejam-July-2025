using UnityEngine;

public sealed class HandCollisionResolver
{
    private const float MinDistanceEpsilon = 0.0001f;
    private const string ReasonFree = "NoContact";
    private const string ReasonPrimaryHit = "PrimarySweepHit";
    private const string ReasonPrimaryBlocked = "PrimaryBlocked";
    private const string ReasonSlideResolved = "SlideResolved";
    private const string ReasonSlideBlocked = "SlideBlocked";
    private const string ReasonDepenetrationGuard = "DepenetrationGuard";

    private readonly Collider _handCollider;
    private readonly CapsuleCollider _capsuleCollider;
    private readonly Rigidbody _ownerRigidbody;
    private readonly Transform _ownerTransform;
    private readonly LayerMask _collisionMask;
    private readonly float _collisionSlideFactor;
    private readonly int _depenetrationIterations;
    private readonly float _skin;
    private readonly Object _logContext;
    private readonly string _ownerName;

    private readonly RaycastHit[] _castHits = new RaycastHit[8];
    private readonly Collider[] _overlapBuffer = new Collider[16];

    private bool _missingCapsuleLogged;

    public HandCollisionResolver(
        Collider handCollider,
        Rigidbody ownerRigidbody,
        Transform ownerTransform,
        LayerMask collisionMask,
        float collisionSlideFactor,
        int depenetrationIterations,
        float skin,
        Object logContext)
    {
        _handCollider = handCollider;
        _capsuleCollider = handCollider as CapsuleCollider;
        _ownerRigidbody = ownerRigidbody;
        _ownerTransform = ownerTransform;
        _collisionMask = collisionMask;
        _collisionSlideFactor = Mathf.Clamp01(collisionSlideFactor);
        _depenetrationIterations = Mathf.Max(1, depenetrationIterations);
        _skin = Mathf.Max(0f, skin);
        _logContext = logContext;
        _ownerName = logContext != null ? logContext.name : nameof(HandCollisionResolver);
    }

    public Vector3 Resolve(
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

        if (_handCollider == null || !_handCollider.enabled || _handCollider.isTrigger)
            return desiredPosition;

        if (_capsuleCollider == null || _capsuleCollider.transform != _ownerTransform)
        {
            if (!_missingCapsuleLogged)
            {
                Debug.LogWarning(
                    $"{nameof(HandCollisionResolver)} on '{_ownerName}' requires a CapsuleCollider on the same object for cast-and-slide.",
                    _logContext);
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

        if (TryCapsuleCast(currentPosition, desiredRotation, direction, distance + _skin, out RaycastHit firstHit))
        {
            float firstMove = Mathf.Max(0f, firstHit.distance - _skin);
            Vector3 firstStep = currentPosition + direction * firstMove;

            state = ArmContactState.Contact;
            contactNormal = firstHit.normal;
            reason = ReasonPrimaryHit;

            Vector3 consumed = direction * firstMove;
            Vector3 remaining = displacement - consumed;
            Vector3 slide = Vector3.ProjectOnPlane(remaining, firstHit.normal) * _collisionSlideFactor;

            float slideDistance = slide.magnitude;
            if (slideDistance > MinDistanceEpsilon)
            {
                Vector3 slideDir = slide / slideDistance;

                if (TryCapsuleCast(firstStep, desiredRotation, slideDir, slideDistance + _skin, out RaycastHit slideHit))
                {
                    float secondMove = Mathf.Max(0f, slideHit.distance - _skin);
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

        for (int iteration = 0; iteration < _depenetrationIterations; iteration++)
        {
            if (!GetCapsuleGeometry(corrected, desiredRotation, out Vector3 pointA, out Vector3 pointB, out float radius))
                break;

            int overlapCount = Physics.OverlapCapsuleNonAlloc(
                pointA,
                pointB,
                radius,
                _overlapBuffer,
                _collisionMask,
                QueryTriggerInteraction.Ignore);

            bool correctedThisIteration = false;

            for (int i = 0; i < overlapCount; i++)
            {
                Collider other = _overlapBuffer[i];
                if (!IsValidCollisionCandidate(other))
                    continue;

                if (!Physics.ComputePenetration(
                        _handCollider,
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

                corrected += depenetrationDirection * (depenetrationDistance + _skin);
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
            _collisionMask,
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

        Vector3 lossyScale = _ownerTransform.lossyScale;
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

        if (other == _handCollider)
            return false;

        if (_ownerRigidbody != null && other.attachedRigidbody == _ownerRigidbody)
            return false;

        if (_handCollider != null && Physics.GetIgnoreLayerCollision(_handCollider.gameObject.layer, other.gameObject.layer))
            return false;

        if (_handCollider != null && Physics.GetIgnoreCollision(_handCollider, other))
            return false;

        return true;
    }
}
