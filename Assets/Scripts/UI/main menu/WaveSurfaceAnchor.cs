using UnityEngine;

public class WaveSurfaceAnchor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SimpleOceanWaves ocean;

    [Header("Anchor position on ocean mesh")]
    [SerializeField] private Vector3 originalLocalPosition;

    [Header("Offset")]
    [SerializeField] private float surfaceOffset = 0.05f;

    [Header("Rotation")]
    [SerializeField] private bool followOceanRotation = true;
    [SerializeField] private bool followWaveNormal = false;

    private void LateUpdate()
    {
        if (ocean == null)
            return;

        Vector3 displacedLocalPoint = ocean.GetDisplacedLocalPoint(originalLocalPosition);
        displacedLocalPoint.y += surfaceOffset;

        Transform oceanTransform = ocean.transform;

        transform.position = oceanTransform.TransformPoint(displacedLocalPoint);

        if (followWaveNormal)
        {
            Vector3 normal = EstimateNormal(originalLocalPosition);
            Vector3 worldNormal = oceanTransform.TransformDirection(normal);

            transform.rotation = Quaternion.LookRotation(oceanTransform.forward, worldNormal);
        }
        else if (followOceanRotation)
        {
            transform.rotation = oceanTransform.rotation;
        }
    }

    private Vector3 EstimateNormal(Vector3 localPoint)
    {
        const float sampleDistance = 0.05f;

        Vector3 center = ocean.GetDisplacedLocalPoint(localPoint);
        Vector3 right = ocean.GetDisplacedLocalPoint(localPoint + Vector3.right * sampleDistance);
        Vector3 forward = ocean.GetDisplacedLocalPoint(localPoint + Vector3.forward * sampleDistance);

        Vector3 tangentX = right - center;
        Vector3 tangentZ = forward - center;

        return Vector3.Cross(tangentZ, tangentX).normalized;
    }

#if UNITY_EDITOR
    private void Reset()
    {
        ocean = GetComponentInParent<SimpleOceanWaves>();

        if (ocean != null)
        {
            originalLocalPosition = ocean.transform.InverseTransformPoint(transform.position);
        }
    }
#endif
}