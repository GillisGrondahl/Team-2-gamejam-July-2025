using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using VContainer;

public class DiegeticMenuButton : MonoBehaviour
{
    private IInputService _input;

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
    private Vector2 mousePosition = Vector2.zero;
    private bool mouseClick = false;

    [Inject]
    private void Construct(IInputService input)
    {
        _input = input;
    }

    private void OnEnable()
    { 
        _input.MousePosition += UpdateMousePosition;
        _input.Interact += UpdateInteraction;
    }

    private void OnDisable()
    {
        
        _input.MousePosition -= UpdateMousePosition;
        _input.Interact -= UpdateInteraction;
    }

    void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

    }

    void Update()
    {
        CheckForInteraction();
    }

    void UpdateMousePosition(Vector2 position) => mousePosition = position;
    void UpdateInteraction(bool click) => mouseClick = click;

    private void CheckForInteraction()
    {
        if (playerCamera == null || 
            buttonCollider == null || 
            (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())) 
            return;

        // Create ray from camera center
        Ray ray = playerCamera.ScreenPointToRay(mousePosition);

        // Check if ray hits this button's collider
        bool hitThisButton = false;
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, raycastLayers))
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
                if (mouseClick && requireMouseButton) //TODO: Migrate to new InputSystem
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