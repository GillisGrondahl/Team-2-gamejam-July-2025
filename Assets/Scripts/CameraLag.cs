using UnityEngine;

public class CameraLag : MonoBehaviour
{
    [Tooltip("How quickly camera catches up to ship rotation.")]
    [SerializeField] private float dampingSpeed = 0.7f;

    [Tooltip("Explicit ship transform. Leave empty to disable ship counter-rotation.")]
    [SerializeField] private Transform shipTransform;

    [Tooltip("Transform to lag. Defaults to this transform.")]
    [SerializeField] private Transform cameraTransform;
    private Vector3 initialLocalPosition;
    private Quaternion initialLocalRotation;

    private void Awake()
    {
        if (cameraTransform == null)
            cameraTransform = transform;

        initialLocalPosition = cameraTransform.localPosition;
        initialLocalRotation = cameraTransform.localRotation;
    }

    private void LateUpdate()
    {
        if (shipTransform == null)
            return;

        Quaternion targetRotation = Quaternion.Inverse(shipTransform.rotation) * initialLocalRotation;
        cameraTransform.localRotation = Quaternion.Slerp(cameraTransform.localRotation, targetRotation, Time.deltaTime * dampingSpeed);
        cameraTransform.localPosition = initialLocalPosition;
    }
}
