using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float distance = 1f;
    [SerializeField] private bool showCursor = false;


    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = showCursor;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            transform.Translate(Vector3.left * distance);

        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            transform.Translate(Vector3.right * distance);
    }
}
