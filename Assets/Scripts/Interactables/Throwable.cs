using UnityEngine;

public class Throwable : MonoBehaviour
{
    Rigidbody _rb;
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    public void Throw(Interactor interactor)
    {
        _rb.linearVelocity = interactor.CurrentVelocity;
    }
}
