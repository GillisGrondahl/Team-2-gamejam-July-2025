using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Interactable : MonoBehaviour
{
    private Transform _target;
    private Rigidbody _rigidbody;
    private Material outlineMaterial;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        outlineMaterial = GetComponent<Renderer>().materials[1];
    }

    private void LateUpdate()
    {
        FollowTarget();
    }

    private void FollowTarget()
    {
        if (_target == null) return;

        transform.position = _target.position;
        transform.rotation = _target.rotation;
    }

    public void SnapTo(Transform target)
    {
        _target = target;
        _rigidbody.isKinematic = true;
        HideOutline();
    }
    public void Release()
    {
        _target = null;
        _rigidbody.isKinematic = false;
    }

    public void ShowOutline()
    {
        outlineMaterial.SetFloat("_OutlineThickness", 1.1f);
    }

    public void HideOutline()
    {
        outlineMaterial.SetFloat("_OutlineThickness", 0f);
    }
}
