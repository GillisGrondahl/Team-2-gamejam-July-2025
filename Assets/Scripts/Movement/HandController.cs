using UnityEngine;
using VContainer;

[DefaultExecutionOrder(-120)]
public class HandController : MonoBehaviour
{
    private IInputService _input;
    public Transform HandPoint => handPoint;

    [Header("Hand Settings")]
    [SerializeField] private Transform handPivot;
    [SerializeField] private Transform handPoint;

    [Header("Angular Limits")]
    [SerializeField] private float sensitivity = 2f;
    [SerializeField] private float minPitch = -45f, maxPitch = 45f;
    [SerializeField] private float minYaw = -60f, maxYaw = 60f;

    [Header("Distance")]
    [SerializeField] private float aimDistance = 0.26f;

    [Header("Reach")]
    [SerializeField] private float handReachMin = -0.6f, handReachMax = 1.8f;
    [SerializeField] private float reachSpeed = 3.5f;

    private float pitch;
    private float yaw;
    private float handZOffset = 0f;
    private float _reach;
    private Vector2 _look;
    public bool IsReachActive => Mathf.Abs(_reach) > 0.001f;

    [Inject]
    private void Construct(IInputService input)
    {
        _input = input;
    }

    private void Start()
    {
        ConfigurePivotRigidbody();

        if (handPivot != null)
        {
            Vector3 angles = handPivot.localEulerAngles;
            yaw = angles.y;
            pitch = angles.x;
        }

        handZOffset = Mathf.Clamp(handZOffset, handReachMin, handReachMax);

        UpdateHandPointPosition();
    }

    private void OnEnable()
    {
        _input.Reach += UpdateReachInput;
        _input.Look += UpdateLookInput;
    }

    private void OnDisable()
    {
        _input.Reach -= UpdateReachInput;
        _input.Look -= UpdateLookInput;
    }

    private void UpdateReachInput(float direction) => _reach = direction;
    private void UpdateLookInput(Vector2 direction) => _look = direction;

    private void Update()
    {
        UpdateAim();
        Reach();
    }

    private void UpdateAim()
    {
        // Look input is the only aim source.
        yaw += _look.x * sensitivity;
        pitch -= _look.y * sensitivity;

        yaw = Mathf.Clamp(yaw, minYaw, maxYaw);
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        if (handPivot != null)
            handPivot.localRotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    private void Reach()
    {
        handZOffset += _reach * reachSpeed * Time.deltaTime;
        handZOffset = Mathf.Clamp(handZOffset, handReachMin, handReachMax);
        UpdateHandPointPosition();
    }

    private void UpdateHandPointPosition()
    {
        if (handPoint == null || handPivot == null)
            return;

        Vector3 desiredPosition =
            handPivot.position +
            handPivot.forward * aimDistance +
            Vector3.forward * handZOffset;

        handPoint.position = desiredPosition;
    }

    private void ConfigurePivotRigidbody()
    {
        if (handPivot == null)
            return;

        if (handPivot.TryGetComponent(out Rigidbody pivotRb))
        {
            pivotRb.isKinematic = true;
            pivotRb.useGravity = false;
            pivotRb.detectCollisions = false;
            pivotRb.linearVelocity = Vector3.zero;
            pivotRb.angularVelocity = Vector3.zero;
        }
    }
}
