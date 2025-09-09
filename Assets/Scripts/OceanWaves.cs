using System.Diagnostics.CodeAnalysis;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SimpleOceanWaves : MonoBehaviour
{
    [Header("Wave Settings")]
    public float waveHeight = 1f;
    public float waveSpeed = 2f;

    [Header("Physics")]
    public bool updateCollider = true;
    public int colliderUpdateInterval = 3; // Update collider every N frames

    private Mesh mesh;
    private MeshCollider meshCollider;
    private Vector3[] originalVertices;
    private Vector3[] vertices;
    private int frameCounter = 0;

    void Start()
    {
        // Get the existing mesh from the MeshFilter
        mesh = GetComponent<MeshFilter>().mesh;

        // Get or add MeshCollider component
        meshCollider = GetComponent<MeshCollider>();
        if (meshCollider == null)
            meshCollider = gameObject.AddComponent<MeshCollider>();

        // Store original vertex positions
        originalVertices = mesh.vertices;
        vertices = new Vector3[originalVertices.Length];
    }

    void Update()
    {
        AnimateWaves();
    }

    void AnimateWaves()
    {
        // Animate existing vertices with wave motion
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 vertex = originalVertices[i];

            // Create wave using sine functions
            float wave = Mathf.Sin(vertex.x * 0.5f + Time.time * waveSpeed) *
                        Mathf.Sin(vertex.z * 0.3f + Time.time * waveSpeed * 0.7f);

            vertices[i] = new Vector3(vertex.x, vertex.y + wave * waveHeight, vertex.z);
        }

        // Update visual mesh every frame
        mesh.vertices = vertices;
        mesh.RecalculateNormals();

        // Update collider mesh only every few frames
        if (updateCollider && meshCollider != null)
        {
            frameCounter++;
            if (frameCounter >= colliderUpdateInterval)
            {
                meshCollider.sharedMesh = null; // Clear first
                meshCollider.sharedMesh = mesh; // Assign updated mesh
                frameCounter = 0; // Reset counter
            }
        }
    }

    //MaybeLater
    //public float GetHeightAtPosition(Vector3 worldPosition)
    //{
    //    Vector3 localPos = transform.InverseTransformPoint(worldPosition);
    //    Debug.Log((Mathf.Sqrt(vertices.Length) - 1));
    //    int x = Mathf.FloorToInt((localPos.x + 0.5f) * (Mathf.Sqrt(vertices.Length) - 1));
    //    int z = Mathf.FloorToInt((localPos.z + 0.5f) * (Mathf.Sqrt(vertices.Length) - 1));

    //    if (x >= Mathf.Sqrt(vertices.Length) || z >= Mathf.Sqrt(vertices.Length)) return 0f;

    //    int index = (int)(z * (Mathf.Sqrt(vertices.Length) - 1) + x);
    //    return vertices[index].y;
    //}
}