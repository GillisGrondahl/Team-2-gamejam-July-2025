using UnityEngine;

public class Floater : MonoBehaviour
{

    [SerializeField] private SimpleOceanWaves oceanWaves;

    //void Update()
    //{
    //    if (oceanWaves == null) return;

    //    float oceanHeight = oceanWaves.GetHeightAtPosition(transform.position);

    //    Vector3 newPosition = transform.position;
    //    newPosition.y = oceanHeight;
    //    //Debug.Log(newPosition);
    //    transform.position = newPosition;
    //}
}
