using UnityEngine;
using UnityEngine.Events;

public class DiegeticMenuButton : MonoBehaviour
{
    [Header("Collider Reference")]
    [SerializeField] private BoxCollider buttonCollider;

    [Header("Events")]
    [SerializeField] private UnityEvent onClick;
    [SerializeField] private UnityEvent onHover;
    [SerializeField] private UnityEvent onHoverExit;

    [Header("Settings")]
    [SerializeField] private LayerMask raycastLayers = -1;
    [SerializeField] private bool requireMouseButton = true; // If false, triggers on any raycast hit
    [SerializeField] Camera playerCamera;

    public bool isHovering = false;

    void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

    }

    void Update()
    {
        CheckForInteraction();
    }

    private void CheckForInteraction()
    {
        if (playerCamera == null || buttonCollider == null) return;

        // Create ray from camera center
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Check if ray hits this button's collider
        bool hitThisButton = false;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, raycastLayers))
        {
            if (hit.collider == buttonCollider)
            {
                hitThisButton = true;

                // Handle hover state
                if (!isHovering)
                {
                    isHovering = true;
                    onHover.Invoke();
                }

                // Handle click
                if (Input.GetMouseButtonDown(0) && requireMouseButton) //TODO: Migrate to new InputSystem
                {
                    onClick.Invoke();
                }
                else if (!requireMouseButton)
                {
                    onClick.Invoke();
                }
            }
        }

        // Handle hover exit
        if (!hitThisButton && isHovering)
        {
            isHovering = false;
            onHoverExit.Invoke();
        }
    }

    // Public methods for external triggering (optional)
    public void TriggerClick()
    {
        onClick.Invoke();
    }

    public void TriggerHover()
    {
        if (!isHovering)
        {
            isHovering = true;
            onHover.Invoke();
        }
    }

    public void TriggerHoverExit()
    {
        if (isHovering)
        {
            isHovering = false;
            onHoverExit.Invoke();
        }
    }



}