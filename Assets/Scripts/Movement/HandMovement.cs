using UnityEngine;

public class HandMovement : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private float sensitivity = 2f;
    [SerializeField] private float smoothSpeed = 10f;
    [SerializeField] private float minPitch = -45f;
    [SerializeField] private float maxPitch = 45f;
    [SerializeField] private float minYaw = -60f;
    [SerializeField] private float maxYaw = 60f;

    [Header("Hand Reach Settings")]
    [SerializeField] private Transform handTransform;
    [SerializeField] private float handReachMax = 1f;
    [SerializeField] private float handReachMin = 0.1f;

    private Rigidbody rb;
    private float pitch = 0f; // Local X rotation
    private float yaw = 0f;   // Local Y rotation
    private Quaternion initialLocalRotation;
    private Vector3 initialLocalPosition;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        // Save the starting local rotation and position (relative to parent)
        initialLocalRotation = transform.localRotation;
        initialLocalPosition = transform.localPosition;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        yaw += mouseX * sensitivity;
        pitch -= mouseY * sensitivity;

        yaw = Mathf.Clamp(yaw, minYaw, maxYaw);
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // Scroll to push/pull hand forward
        if (Input.mouseScrollDelta.y != 0)
        {
            float scroll = Input.mouseScrollDelta.y * 0.1f;
            Vector3 localPos = handTransform.localPosition + Vector3.forward * scroll;
            localPos.z = Mathf.Clamp(localPos.z, handReachMin, handReachMax);
            handTransform.localPosition = new Vector3(0f, 0f, localPos.z);
        }
    }

    void FixedUpdate()
    {
        if (transform.parent == null)
            return;

        // Compute target local rotation (relative to parent)
        Quaternion localRotation = Quaternion.Euler(pitch, yaw, 0f);
        Quaternion targetWorldRotation = transform.parent.rotation * localRotation;

        // Apply physics-safe rotation
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetWorldRotation, smoothSpeed * Time.fixedDeltaTime));

        //// Stay at intended local position (optional)
        //Vector3 targetWorldPosition = transform.parent.TransformPoint(initialLocalPosition);
        //rb.MovePosition(targetWorldPosition);
    }
}
