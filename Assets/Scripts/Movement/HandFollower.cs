using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class HandFollower : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float positionStiffness = 30f;
    [SerializeField] private float rotationStiffness = 10f;
    [SerializeField] private float maxLinearSpeed = 10f;
    [SerializeField] private float maxAngularSpeed = 20f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void FixedUpdate()
    {
        if (target == null) return;

        // Position
        Vector3 toTarget = target.position - rb.position;
        Vector3 desiredVelocity = toTarget * positionStiffness;
        rb.linearVelocity = Vector3.ClampMagnitude(desiredVelocity, maxLinearSpeed);

        // Rotation
        Quaternion deltaRot = target.rotation * Quaternion.Inverse(rb.rotation);
        deltaRot.ToAngleAxis(out float angleInDegrees, out Vector3 rotationAxis);
        if (angleInDegrees > 180f) angleInDegrees -= 360f;

        float angleInRadians = angleInDegrees * Mathf.Deg2Rad;
        Vector3 desiredAngularVel = rotationAxis.normalized * angleInRadians * rotationStiffness;
        rb.angularVelocity = Vector3.ClampMagnitude(desiredAngularVel, maxAngularSpeed);
    }
}
