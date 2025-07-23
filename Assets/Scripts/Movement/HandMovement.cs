using UnityEngine;

public class HandMovement : MonoBehaviour
{
    [SerializeField] private Transform handTransform;
    [SerializeField] private float handRangeMax = 1f;
    [SerializeField] private float handRangeMin = 0.1f;
    [SerializeField] private float sensitivity = 10f;

    Camera mainCamera;
    Vector3 mousePos;

    void Update()
    {
        //float mouseX = Input.GetAxis("Mouse X");
        //float mouseY = Input.GetAxis("Mouse Y");
        //float mouseZ = Vector3.Distance(Camera.main.transform.position, transform.position);


        //Vector3 mouseScreenPos = new Vector3(mouseX, mouseY, mouseZ);
        ////mouseScreenPos.z = Vector3.Distance(Camera.main.transform.position, transform.position);

        //Vector3 worldTarget = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        //transform.LookAt(worldTarget);

        float deltaX = Input.GetAxis("Mouse X");
        float deltaY = Input.GetAxis("Mouse Y");

        // Move or rotate something with that delta
        transform.Rotate(Vector3.up, deltaX * sensitivity);
        transform.Rotate(Vector3.left, deltaY * sensitivity);

        //Vector3 delta = new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), 0);
        //handTransform.position += delta * sensitivity;

        if (Input.mouseScrollDelta.y != 0)
        {
            float scrollAmount = Input.mouseScrollDelta.y * 0.1f;
            handTransform.transform.localPosition += transform.forward * scrollAmount;

            float handZPos = handTransform.transform.localPosition.z;
            if (handZPos >= handRangeMax)
            {
                handZPos = handRangeMax;
            }
            else if (handZPos <= handRangeMin)
            {
                handZPos = handRangeMin;
            }

            handTransform.transform.localPosition =
                new Vector3(0f, 0f, handZPos);
        }
    }
}
