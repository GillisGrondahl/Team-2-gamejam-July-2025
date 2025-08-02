using UnityEngine;

[ExecuteAlways]
public class DrawGizmo : MonoBehaviour
{
    public Color gizmoColor = Color.yellow;
    public bool drawWhenSelectedOnly = true;

    void OnDrawGizmos()
    {
        if (drawWhenSelectedOnly) return;
        DrawColliderGizmo();
    }

    void OnDrawGizmosSelected()
    {
        if (!drawWhenSelectedOnly) return;
        DrawColliderGizmo();
    }

    private void DrawColliderGizmo()
    {
        Gizmos.color = gizmoColor;
        Collider col = GetComponent<Collider>();
        if (col == null) return;

        Gizmos.matrix = transform.localToWorldMatrix;

        switch (col)
        {
            case BoxCollider box:
                Gizmos.DrawWireCube(box.center, box.size);
                break;

            case SphereCollider sphere:
                Gizmos.DrawWireSphere(sphere.center, sphere.radius);
                break;

            case CapsuleCollider capsule:
                DrawWireCapsule(capsule);
                break;

            case MeshCollider mesh:
                if (mesh.sharedMesh != null)
                    Gizmos.DrawWireMesh(mesh.sharedMesh, Vector3.zero, Quaternion.identity, Vector3.one);
                break;
        }

        Gizmos.matrix = Matrix4x4.identity;
    }

    private void DrawWireCapsule(CapsuleCollider capsule)
    {
        // Approximate drawing by using Gizmos.DrawWireSphere and lines (not perfect)
        Vector3 center = capsule.center;
        float radius = capsule.radius;
        float height = Mathf.Max(capsule.height, radius * 2f);

        int direction = capsule.direction;
        Vector3 up = Vector3.up;
        if (direction == 0) up = Vector3.right;
        else if (direction == 2) up = Vector3.forward;

        float cylinderHeight = height - 2 * radius;
        Vector3 top = center + up * (cylinderHeight / 2f);
        Vector3 bottom = center - up * (cylinderHeight / 2f);

        Gizmos.DrawWireSphere(top, radius);
        Gizmos.DrawWireSphere(bottom, radius);
        Gizmos.DrawLine(top + Quaternion.Euler(0, 0, 0) * (Vector3.right * radius), bottom + Quaternion.Euler(0, 0, 0) * (Vector3.right * radius));
        Gizmos.DrawLine(top + Quaternion.Euler(0, 0, 0) * (Vector3.forward * radius), bottom + Quaternion.Euler(0, 0, 0) * (Vector3.forward * radius));
        Gizmos.DrawLine(top + Quaternion.Euler(0, 0, 0) * (-Vector3.right * radius), bottom + Quaternion.Euler(0, 0, 0) * (-Vector3.right * radius));
        Gizmos.DrawLine(top + Quaternion.Euler(0, 0, 0) * (-Vector3.forward * radius), bottom + Quaternion.Euler(0, 0, 0) * (-Vector3.forward * radius));
    }
}
