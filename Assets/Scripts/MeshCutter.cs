using UnityEngine;
using System.Collections.Generic;

public class MeshCutter
{
    public static void Cut(GameObject target, Plane plane, Material capMaterial = null)
    {
        var mesh = target.GetComponent<MeshFilter>().mesh;
        var verts = mesh.vertices;
        var tris = mesh.triangles;

        Matrix4x4 localToWorld = target.transform.localToWorldMatrix;
        Matrix4x4 worldToLocal = target.transform.worldToLocalMatrix;

        List<Vector3> leftVerts = new(), rightVerts = new();
        List<int> leftTris = new(), rightTris = new();

        for (int i = 0; i < tris.Length; i += 3)
        {
            int i0 = tris[i], i1 = tris[i + 1], i2 = tris[i + 2];
            Vector3 w0 = localToWorld.MultiplyPoint3x4(verts[i0]);
            Vector3 w1 = localToWorld.MultiplyPoint3x4(verts[i1]);
            Vector3 w2 = localToWorld.MultiplyPoint3x4(verts[i2]);

            bool s0 = plane.GetSide(w0);
            bool s1 = plane.GetSide(w1);
            bool s2 = plane.GetSide(w2);

            if (s0 == s1 && s1 == s2)
            {
                AddTriangle(s0 ? rightVerts : leftVerts, s0 ? rightTris : leftTris, w0, w1, w2, worldToLocal);
            }
            else
            {
                SliceTriangle(plane, w0, w1, w2, s0, s1, s2, leftVerts, leftTris, rightVerts, rightTris, worldToLocal);
            }
        }

        CreatePart("LeftPart", leftVerts, leftTris, target.transform, capMaterial);
        CreatePart("RightPart", rightVerts, rightTris, target.transform, capMaterial);

        Object.Destroy(target);
    }

    static void AddTriangle(List<Vector3> vertList, List<int> triList, Vector3 a, Vector3 b, Vector3 c, Matrix4x4 toLocal)
    {
        int index = vertList.Count;
        vertList.Add(toLocal.MultiplyPoint3x4(a));
        vertList.Add(toLocal.MultiplyPoint3x4(b));
        vertList.Add(toLocal.MultiplyPoint3x4(c));
        triList.Add(index);
        triList.Add(index + 1);
        triList.Add(index + 2);
    }

    static void SliceTriangle(
        Plane plane,
        Vector3 v0, Vector3 v1, Vector3 v2,
        bool s0, bool s1, bool s2,
        List<Vector3> leftVerts, List<int> leftTris,
        List<Vector3> rightVerts, List<int> rightTris,
        Matrix4x4 toLocal)
    {
        Vector3 solo, pair0, pair1;
        bool soloSide;

        if (s0 != s1 && s0 != s2)
        {
            solo = v0; soloSide = s0;
            pair0 = v1; pair1 = v2;
        }
        else if (s1 != s0 && s1 != s2)
        {
            solo = v1; soloSide = s1;
            pair0 = v0; pair1 = v2;
        }
        else
        {
            solo = v2; soloSide = s2;
            pair0 = v0; pair1 = v1;
        }

        Vector3 i0 = IntersectEdge(solo, pair0, plane);
        Vector3 i1 = IntersectEdge(solo, pair1, plane);

        if (soloSide)
        {
            AddTriangle(rightVerts, rightTris, solo, i0, i1, toLocal);
            AddTriangle(leftVerts, leftTris, i0, pair0, pair1, toLocal);
            AddTriangle(leftVerts, leftTris, i0, pair1, i1, toLocal);
        }
        else
        {
            AddTriangle(leftVerts, leftTris, solo, i0, i1, toLocal);
            AddTriangle(rightVerts, rightTris, i0, pair0, pair1, toLocal);
            AddTriangle(rightVerts, rightTris, i0, pair1, i1, toLocal);
        }
    }

    static Vector3 IntersectEdge(Vector3 a, Vector3 b, Plane plane)
    {
        Vector3 dir = b - a;
        plane.Raycast(new Ray(a, dir), out float enter);
        return a + dir.normalized * enter;
    }

    static void CreatePart(string name, List<Vector3> verts, List<int> tris, Transform baseTransform, Material mat)
    {
        GameObject go = new(name);
        Mesh m = new();
        m.SetVertices(verts);
        m.SetTriangles(tris, 0);
        m.RecalculateNormals();

        var rb = go.AddComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        var collider = go.AddComponent<MeshCollider>();
        collider.sharedMesh = m;
        collider.convex = true;

        var mf = go.AddComponent<MeshFilter>();
        var mr = go.AddComponent<MeshRenderer>();
        mf.mesh = m;
        mr.material = mat ?? new Material(Shader.Find("Standard"));

        go.transform.SetParent(baseTransform.parent);
        go.transform.position = baseTransform.position;
        go.transform.rotation = baseTransform.rotation;
        go.transform.localScale = baseTransform.localScale;
    }
}