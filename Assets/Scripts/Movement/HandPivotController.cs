using UnityEngine;

public class HandPivotController : MonoBehaviour
{
    [SerializeField] private float sensitivity = 2f;
    [SerializeField] private float minPitch = -45f, maxPitch = 45f;
    [SerializeField] private float minYaw = -60f, maxYaw = 60f;
    [SerializeField] private Transform handPoint;
    [SerializeField] private float handReachMin = 0.1f, handReachMax = 1.0f;
    [SerializeField] private float scrollSpeed = 0.1f;

    private float pitch = 0f, yaw = 0f;
    private float handZOffset = 0.5f;

    void Start()
    {
        Vector3 angles = transform.localEulerAngles;
        yaw = angles.y;
        pitch = angles.x;
        UpdateHandPointPosition();
    }

    void Update()
    {
        float dx = Input.GetAxis("Mouse X");
        float dy = Input.GetAxis("Mouse Y");

        yaw += dx * sensitivity;
        pitch -= dy * sensitivity;

        yaw = Mathf.Clamp(yaw, minYaw, maxYaw);
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        transform.localRotation = Quaternion.Euler(pitch, yaw, 0f);

        if (Input.mouseScrollDelta.y != 0)
        {
            handZOffset += Input.mouseScrollDelta.y * scrollSpeed;
            handZOffset = Mathf.Clamp(handZOffset, handReachMin, handReachMax);
            UpdateHandPointPosition();
        }
    }

    private void UpdateHandPointPosition()
    {
        if (handPoint != null)
            handPoint.localPosition = new Vector3(0f, 0f, handZOffset);
    }
}
