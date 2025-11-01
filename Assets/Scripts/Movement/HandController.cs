using UnityEngine;
using VContainer;

public class HandController : MonoBehaviour
{
    private IInputService _input;

    [Header("Hand Settings")]
    [SerializeField] private Transform handPivot;
    [SerializeField] private float sensitivity = 2f;
    [SerializeField] private float minPitch = -45f, maxPitch = 45f;
    [SerializeField] private float minYaw = -60f, maxYaw = 60f;
    [SerializeField] private Transform handPoint;
    [SerializeField] private float handReachMin = 0.1f, handReachMax = 1.0f;
    [SerializeField] private float reachSpeed = 0.1f;

    private float pitch = 0f, yaw = 0f;
    private float handZOffset = 0.5f;
    private float _reach = 0f;
    private Vector2 _look = Vector2.zero;

    [Inject]
    private void Construct(IInputService input)
    {
        _input = input;
    }

    void Start()
    {
        Vector3 angles = handPivot.transform.localEulerAngles;
        yaw = angles.y;
        pitch = angles.x;
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

    void Update()
    {
        HandMovement();
        Reach();
    }

    private void HandMovement()
    {
        yaw += _look.x * sensitivity;
        pitch -= _look.y * sensitivity;

        yaw = Mathf.Clamp(yaw, minYaw, maxYaw);
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        handPivot.transform.localRotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    private void Reach()
    {
        handZOffset += _reach * reachSpeed;
        handZOffset = Mathf.Clamp(handZOffset, handReachMin, handReachMax);
        UpdateHandPointPosition();
    }

    private void UpdateHandPointPosition()
    {
        if (handPoint != null)
            handPoint.localPosition = new Vector3(0f, 0f, handZOffset);
    }
}
