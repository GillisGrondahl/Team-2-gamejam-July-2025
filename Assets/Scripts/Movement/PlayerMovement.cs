using MoreMountains.Tools;
using System.Net.NetworkInformation;
using Unity.VisualScripting;
using UnityEngine;
using VContainer;

public class PlayerMovement : MonoBehaviour
{
    private IInputService _input;

    [Header("Zone Restriction")]
    [SerializeField] private BoxCollider playerZone;  // Reference to the PlayerZone collider
    [SerializeField] private BoxCollider oneArmedScreenScrollLeft;
    [SerializeField] private BoxCollider oneArmedScreenScrollRight;
    [SerializeField] private SphereCollider interactorCollider;

    [Header("Movement")]
    [SerializeField] private float maxSpeed = 2f;
    [SerializeField] private float acceleration = 2f;
    [SerializeField] private float deceleration = 2f;
    [SerializeField] private bool showCursor = false;
    [SerializeField] private bool oneArmedMode = false;

    [Header("Jitter")]
    [SerializeField] private bool jitterEnabled = true;
    [SerializeField] private float jitterIntensity = 0.7f;
    [SerializeField] private float jitterFrequency = 1.1f;

    private float currentVelocity = 0f;
    private float inputDirection = 0f;
    private float jitterOffset = 0f;

    // for one-armed mode
    private bool isRMBHeld = false;
    private bool hasInitialMousePosition = false;
    private float keyboardInput = 0f;
    private float mouseInput = 0f;


    [Inject]
    private void Construct(IInputService input)
    {
        _input = input;
    }

    private void Start()
    {
        // Subscribe to one-armed mode event
        GameEvents.Instance.OnOneArmedModeToggled += HandleOneArmedModeToggled;

        Cursor.lockState = showCursor ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = showCursor;

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
    }

    private void OnDisable()
    {
        _input.Move -= UpdateDirection;
        _input.OneArmedRMB -= HandleRMBState;
    }

    private void Update()
    {
        UpdateMovement();

        if (jitterEnabled)
        {
            ApplyJitter();
        }
    }

    #region OneArmedMode
    private void HandleOneArmedModeToggled(bool oneArmedMode)
    {
        this.oneArmedMode = oneArmedMode;

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

    private void UpdateMovement()
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
        currentVelocity = Mathf.MoveTowards(currentVelocity, targetVelocity, accelerationRate * Time.deltaTime);

        if (Mathf.Abs(currentVelocity) > 0.01f)
        {
            Vector3 movement = Vector3.right * currentVelocity * Time.deltaTime;
            Vector3 newPosition = transform.position + movement;

            bool currentlyInZone = IsWithinPlayerZone(transform.position);
            bool newPositionInZone = IsWithinPlayerZone(newPosition);

            if (newPositionInZone || (!currentlyInZone && IsMovingTowardsZone(movement)))
            {
                transform.Translate(movement);
            }
            else
            {
                currentVelocity = 0f;
            }
        }
    }

    private void ApplyJitter()
    {
        float noiseValue = Mathf.PerlinNoise(Time.time * jitterFrequency, 0f);
        jitterOffset = (noiseValue - 0.5f) * 2f * jitterIntensity;

        Vector3 jitterMovement = Vector3.right * jitterOffset * Time.deltaTime;
        Vector3 newPosition = transform.position + jitterMovement;

        // Only apply jitter if it keeps player in bounds or moves them back in
        if (IsWithinPlayerZone(newPosition) ||
            (!IsWithinPlayerZone(transform.position) && IsMovingTowardsZone(jitterMovement)))
        {
            transform.Translate(jitterMovement);
        }
    }

    private bool IsWithinPlayerZone(Vector3 position)
    {
        if (playerZone == null) return true; // No restriction if zone not found

        return playerZone.bounds.Contains(position);
    }

    private bool IsMovingTowardsZone(Vector3 movement)
    {
        if (playerZone == null) return true;

        Vector3 currentPos = transform.position;
        Vector3 zoneCenter = playerZone.bounds.center;

        // Calculate distance to zone center before and after movement
        float currentDistance = Vector3.Distance(currentPos, zoneCenter);
        float newDistance = Vector3.Distance(currentPos + movement, zoneCenter);

        // Movement is towards zone if new distance is smaller
        return newDistance < currentDistance;
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