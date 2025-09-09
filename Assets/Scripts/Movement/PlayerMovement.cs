using System.Net.NetworkInformation;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Inputs")]
    [SerializeField] private InputReader input;

    [Header("Zone Restriction")]
    [SerializeField] private BoxCollider playerZone;  // Reference to the PlayerZone collider

    [Header("Movement")]
    [SerializeField] private float maxSpeed = 2f;
    [SerializeField] private float acceleration = 2f;
    [SerializeField] private float deceleration = 2f;
    [SerializeField] private bool showCursor = false;

    [Header("Jitter")]
    [SerializeField] private bool jitterEnabled = true;
    [SerializeField] private float jitterIntensity = 0.7f;
    [SerializeField] private float jitterFrequency = 1.1f;

    private float currentVelocity = 0f;
    private float inputDirection = 0f;
    private float jitterOffset = 0f;


    private void Start()
    {
        input.EnableInputActions();
        Cursor.lockState = showCursor? CursorLockMode.None : CursorLockMode.Locked;
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
        input.Move += UpdateDirection;
    }

    private void OnDisable()
    {
        input.Move -= UpdateDirection;
    }

    private void Update()
    {
        UpdateMovement();
        if (jitterEnabled)
        {
            ApplyJitter();
        }
    }

    private void UpdateDirection(Vector2 direction) => inputDirection = direction.x; 

    private void UpdateMovement()
    {
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
                // Stop movement if trying to go further out of bounds
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


}