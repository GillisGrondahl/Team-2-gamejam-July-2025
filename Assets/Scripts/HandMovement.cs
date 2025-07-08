using UnityEngine;

public class HandMovement : MonoBehaviour
{
    [SerializeField] private Transform handTransform;
    [SerializeField] private float handRangeMax = 1f;
    [SerializeField] private float handRangeMin = 0.1f;

    Camera mainCamera;
    Vector3 mousePos;

    void Update()
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Vector3.Distance(Camera.main.transform.position, transform.position);

        Vector3 worldTarget = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        transform.LookAt(worldTarget);

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
