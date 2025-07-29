using MoreMountains.Feedbacks;
using System.Collections;
using UnityEngine;

public class Blade : MonoBehaviour
{
    [SerializeField] private MMF_Player _MMFCutting;

    private bool _isKnifing = false;
    //private GameObject originalGameObject = null;
    //private Vector3 cutNormal;
    //private Vector3 contactPoint;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out Ingredient ingredient) && !_isKnifing)
        {
            //Debug.Log("Trigger enter Ingredient: " + ingredient.name);
            //originalGameObject = ingredient.gameObject;
            //cutNormal = transform.right;
            //contactPoint = transform.position;

            _isKnifing = true;
            Cutter.Cut(ingredient.gameObject, transform.position, transform.right);
            StartCoroutine("WaitForNextSlice");

            // call MMF Feedback for cutting sounds
            _MMFCutting.PlayFeedbacks();
        }
    }
    private IEnumerator WaitForNextSlice()
    {
        yield return new WaitForSeconds(1f);
        _isKnifing = false;
    }

    //void OnDrawGizmosSelected()
    //{
    //    if (originalGameObject == null) return;

    //    // Compute the world-space normal and point
    //    Vector3 normal = originalGameObject.transform.TransformDirection(cutNormal.normalized);
    //    Vector3 point = contactPoint;

    //    // Draw the contact point
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawSphere(point, 0.01f);

    //    // Draw the plane normal
    //    Gizmos.color = Color.green;
    //    Gizmos.DrawLine(point, point + normal * 0.3f);

    //    // Draw a visual representation of the plane
    //    DrawPlaneGizmo(point, normal, 0.2f);
    //}

    //void DrawPlaneGizmo(Vector3 center, Vector3 normal, float size)
    //{
    //    Vector3 tangent = Vector3.Cross(normal, Vector3.up);
    //    if (tangent.sqrMagnitude < 0.01f)
    //        tangent = Vector3.Cross(normal, Vector3.right);
    //    tangent.Normalize();
    //    Vector3 bitangent = Vector3.Cross(normal, tangent);

    //    Vector3 corner0 = center + (tangent + bitangent) * size;
    //    Vector3 corner1 = center + (tangent - bitangent) * size;
    //    Vector3 corner2 = center + (-tangent - bitangent) * size;
    //    Vector3 corner3 = center + (-tangent + bitangent) * size;

    //    Gizmos.color = Color.cyan;
    //    Gizmos.DrawLine(corner0, corner1);
    //    Gizmos.DrawLine(corner1, corner2);
    //    Gizmos.DrawLine(corner2, corner3);
    //    Gizmos.DrawLine(corner3, corner0);
    //}
}
