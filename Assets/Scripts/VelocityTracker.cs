using UnityEngine;

public class VelocityTracker : MonoBehaviour
{
    public Vector3 Velocity { get; private set; }
    public bool IsMovingDownward { get; private set; }

    private Vector3 _lastPosition;

    void Start()
    {
        _lastPosition = transform.position;
    }

    void LateUpdate()
    {
        Velocity = (transform.position - _lastPosition) / Time.deltaTime;
        _lastPosition = transform.position;

        IsMovingDownward = Vector3.Dot(Velocity.normalized, Vector3.down) > 0.7f;
    }
}
