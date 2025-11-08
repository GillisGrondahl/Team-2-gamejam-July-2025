using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class HandFollower : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float positionStiffness = 30f;
    [SerializeField] private float rotationStiffness = 10f;
    [SerializeField] private float maxLinearSpeed = 10f;
    [SerializeField] private float maxAngularSpeed = 20f;

    //[SerializeField] private Transform handOpen = null;
    //[SerializeField] private Transform handClosed = null;

    [SerializeField] private Animator animator;
    
    private bool isClosing = false;

    private Rigidbody rb;

    void Start()
    {
        animator.Play("HandClose", 0, 0f);
        animator.speed = 0f;
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

    
    public void CloseHand(bool close)
    {
        // handOpen.gameObject.SetActive(!close);
        // handClosed.gameObject.SetActive(close);

        if (close && !isClosing)
        {
            animator.speed = 1.5f;                 
            animator.Play("HandClose", 0, 0f);   
        }
        else
        {
            animator.speed = 2.5f;                
            animator.Play("HandOpen", 0, 0f);
            //StartCoroutine(ResetToOpen());
        }

        isClosing = close;
    }
 
}
