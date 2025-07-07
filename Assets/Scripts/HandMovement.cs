using UnityEngine;

public class HandMovement : MonoBehaviour
{
    Camera mainCamera;
    Vector3 mousePos;


    // Update is called once per frame
    void Update()
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Vector3.Distance(Camera.main.transform.position, transform.position);

        Vector3 worldTarget = Camera.main.ScreenToWorldPoint(mouseScreenPos);

        // point arm toward it
        transform.LookAt(worldTarget);

        // or if it’s just a tip that moves freely:
        //blueTipTransform.position = worldTarget;

    }
}
