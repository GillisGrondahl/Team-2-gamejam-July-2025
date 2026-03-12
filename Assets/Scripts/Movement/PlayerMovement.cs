using UnityEngine;
using VContainer;

[DefaultExecutionOrder(-200)]
public class PlayerMovement : MonoBehaviour
{
    

    [Header("Zone Restriction")]
    [SerializeField] private BoxCollider playerZone;  // Reference to the PlayerZone collider
    [SerializeField] private BoxCollider oneArmedScreenScrollLeft;
    [SerializeField] private BoxCollider oneArmedScreenScrollRight;
    [SerializeField] private SphereCollider interactorCollider;

    [Header("Movement")]
    [SerializeField] private float maxSpeed = 2f;
    [SerializeField] private float acceleration = 2f;
    [SerializeField] private float deceleration = 2f;
    [SerializeField] private bool oneArmedMode = false;

    [Header("Jitter")]
    [SerializeField] private bool jitterEnabled = false;
    [SerializeField] private float jitterIntensity = 0.7f;
    [SerializeField] private float jitterFrequency = 1.1f;

    private float currentVelocity = 0f;
    private float inputDirection = 0f;
    private float jitterOffset = 0f;

    // for one-armed mode
    private bool isRMBHeld = false;
    private float keyboardInput = 0f;
    private float mouseInput = 0f;

    private IInputService _input;
    private ISettingsService _settings;

    [Inject]
    private void Construct(IInputService input, ISettingsService settings)
    {
        _input = input;
        _settings = settings;
    }

    private void Start()
    {
        HandleOneArmedModeChange(_settings.Current.Gameplay);

        if (playerZone == null)
        {
            GameObject zoneObject = GameObject.FindWithTag("PlayerZone");
            if (zoneObject != null)
            {
                playerZone = zoneObject.GetComponent<BoxCollider>();
            }
        }
    }

    private void OnEnable()
    {
        _input.Move += UpdateDirection;
        _input.OneArmedRMB += HandleRMBState; // for one-armed mode
        _settings.GameplaySettingsChanged += HandleOneArmedModeChange;
    }

    private void OnDisable()
    {
        _input.Move -= UpdateDirection;
        _input.OneArmedRMB -= HandleRMBState;
        _settings.GameplaySettingsChanged -= HandleOneArmedModeChange;
    }

    private void Update() { }

    private void FixedUpdate()
    {
        UpdateMovement(Time.fixedDeltaTime);

        if (jitterEnabled)
        {
            ApplyJitter(Time.fixedTime, Time.fixedDeltaTime);
        }
    }

    #region OneArmedMode
    private void HandleOneArmedModeChange(GameplaySettings gameplaySettings)
    {
        oneArmedMode = gameplaySettings.OneArmedMode;

    }
    private void HandleRMBState(bool isPressed)
    {
        isRMBHeld = isPressed;
    }

    #endregion

    private void UpdateDirection(Vector2 direction)
    {
        keyboardInput = direction.x;
    }

    private void UpdateMovement(float dt)
    {
        // Determine which input source to use
        if (oneArmedMode && isRMBHeld)
        {
            // Check for collision with left or right screen scroll zones
            bool collidingLeft = IsCollidingWith(interactorCollider, oneArmedScreenScrollLeft);
            bool collidingRight = IsCollidingWith(interactorCollider, oneArmedScreenScrollRight);

            if (collidingLeft)
            {
                mouseInput = -1f; // Move left
            }
            else if (collidingRight)
            {
                mouseInput = 1f; // Move right
            }
            else
            {
                mouseInput = 0f; // No movement
            }

            inputDirection = mouseInput;
        }
        else
        {
            inputDirection = keyboardInput;
        }

        float targetVelocity = inputDirection * maxSpeed;
        float accelerationRate = (inputDirection != 0f) ? acceleration : deceleration;
        currentVelocity = Mathf.MoveTowards(currentVelocity, targetVelocity, accelerationRate * dt);

        if (Mathf.Abs(currentVelocity) > 0.01f)
        {
            Vector3 movementAxis = GetMovementAxis();
            Vector3 movement = movementAxis * currentVelocity * dt;
            Vector3 newPosition = transform.position + movement;

            bool currentlyInZone = IsWithinPlayerZone(transform.position);
            bool newPositionInZone = IsWithinPlayerZone(newPosition);

            if (newPositionInZone || (!currentlyInZone && IsMovingTowardsZone(movement)))
            {
                transform.position = newPosition;
            }
            else
            {
                currentVelocity = 0f;
            }
        }
    }

    private void ApplyJitter(float currentTime, float dt)
    {
        float noiseValue = Mathf.PerlinNoise(currentTime * jitterFrequency, 0f);
        jitterOffset = (noiseValue - 0.5f) * 2f * jitterIntensity;

        Vector3 jitterMovement = GetMovementAxis() * jitterOffset * dt;
        Vector3 newPosition = transform.position + jitterMovement;

        // Only apply jitter if it keeps player in bounds or moves them back in
        if (IsWithinPlayerZone(newPosition) ||
            (!IsWithinPlayerZone(transform.position) && IsMovingTowardsZone(jitterMovement)))
        {
            transform.position = newPosition;
        }
    }

    private bool IsWithinPlayerZone(Vector3 position)
    {
        if (playerZone == null) return true; // No restriction if zone not found

        Vector3 closestPoint = playerZone.ClosestPoint(position);
        return (closestPoint - position).sqrMagnitude <= 0.000001f;
    }

    private bool IsMovingTowardsZone(Vector3 movement)
    {
        if (playerZone == null) return true;

        Vector3 currentPos = transform.position;
        Vector3 nextPos = currentPos + movement;

        Vector3 currentClosest = playerZone.ClosestPoint(currentPos);
        Vector3 nextClosest = playerZone.ClosestPoint(nextPos);

        float currentDistance = (currentClosest - currentPos).sqrMagnitude;
        float newDistance = (nextClosest - nextPos).sqrMagnitude;

        // Movement is towards zone if new distance is smaller
        return newDistance < currentDistance;
    }

    private Vector3 GetMovementAxis()
    {
        if (playerZone != null)
            return playerZone.transform.right.normalized;

        if (transform.parent != null)
            return transform.parent.right.normalized;

        return transform.right.normalized;
    }

    // Helper method to check collision between two colliders
    private bool IsCollidingWith(SphereCollider sphere, BoxCollider box)
    {
        if (sphere == null || box == null) return false;

        Vector3 closestPoint = box.ClosestPoint(sphere.transform.position);
        float distance = Vector3.Distance(closestPoint, sphere.transform.position);
        float sphereRadius = sphere.radius * sphere.transform.lossyScale.x;

        return distance <= sphereRadius;
    }

}
