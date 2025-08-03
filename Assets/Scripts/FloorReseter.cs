using UnityEngine;

public class FloorReseter : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.TryGetComponent<Tool>(out var tool))
        {
            tool.ResetPosition();
        }
    }
}
