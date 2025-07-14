using UnityEngine;

public class CameraLag : MonoBehaviour
{
    [Tooltip("How quickly camera catches up to ship rotation")]
    public float dampingSpeed = 0.7f;

    private Transform shipTransform;
    private Vector3 initialLocalPosition;
    private Quaternion initialLocalRotation;

    void Start()
    {
        shipTransform = transform.parent.parent; // assuming Ship -> Player -> Camera

        initialLocalPosition = transform.localPosition;
        initialLocalRotation = transform.localRotation;


    }

    void Update()
    {
        // Target rotation is "opposite" to ship's current rotation change
        Quaternion targetRotation = Quaternion.Inverse(shipTransform.rotation) * initialLocalRotation;

        // Dampen rotation using dampingSpeed
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * dampingSpeed);
        transform.localPosition = initialLocalPosition;
    }
}