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

    public float GetWaveHeightLocal(Vector3 localPoint)
    {
        float wave =
            Mathf.Sin(localPoint.x * 0.5f + Time.time * waveSpeed) *
            Mathf.Sin(localPoint.z * 0.3f + Time.time * waveSpeed * 0.7f);

        return wave * waveHeight;
    }

    public Vector3 GetDisplacedLocalPoint(Vector3 originalLocalPoint)
    {
        originalLocalPoint.y += GetWaveHeightLocal(originalLocalPoint);
        return originalLocalPoint;
    }

    private void AnimateWaves()
    {
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 vertex = originalVertices[i];

            vertices[i] = GetDisplacedLocalPoint(vertex);
        }

        mesh.vertices = vertices;
        mesh.RecalculateNormals();

        if (updateCollider && meshCollider != null)
        {
            frameCounter++;

            if (frameCounter >= colliderUpdateInterval)
            {
                meshCollider.sharedMesh = null;
                meshCollider.sharedMesh = mesh;
                frameCounter = 0;
            }
        }
    }
}