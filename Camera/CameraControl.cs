using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 20f;
    public float mouseSensitivity = 0.5f; // ”—⁄… ”Õ» «·„«Ê”
    public float smoothing = 10f;

    [Header("Rotation")]
    public float rotationSpeed = 100f;
    private float currentYRotation = 0f;

    [Header("Zoom")]
    public float zoomSpeed = 500f;
    public float minHeight = 5f;
    public float maxHeight = 40f;

    private Vector3 targetPosition;
    private float targetTilt = 54f;
    private Vector3 lastMousePosition; // · Œ“Ì‰ „ﬂ«‰ «·„«Ê” ›Ì «·›—Ì„ «·”«»ﬁ

    void Start()
    {
        targetPosition = transform.position;
        currentYRotation = transform.eulerAngles.y;
        targetTilt = transform.eulerAngles.x;
    }

    void Update()
    {
        HandleKeyboardMovement();
        HandleMousePan(); // «·œ«·… «·ÃœÌœ… ··”Õ» »«·Ì„Ì‰
        HandleRotation();
        HandleZoom();

        //  ÿ»Ìﬁ «·Õ—ﬂ… «·„‰⁄„…
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smoothing);

        Quaternion targetRot = Quaternion.Euler(targetTilt, currentYRotation, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * smoothing);
    }

    void HandleKeyboardMovement()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 forward = transform.forward;
        forward.y = 0;
        Vector3 right = transform.right;

        Vector3 moveDir = (forward * z) + (right * x);
        targetPosition += moveDir.normalized * moveSpeed * Time.deltaTime;
    }

    void HandleMousePan()
    {
        // ⁄‰œ «·÷€ÿ ·√Ê· „—… ﬂ·Ìﬂ Ì„Ì‰
        if (Input.GetMouseButtonDown(1))
        {
            lastMousePosition = Input.mousePosition;
        }

        // √À‰«¡ «·«” „—«— »«·÷€ÿ ⁄·Ï ﬂ·Ìﬂ Ì„Ì‰
        if (Input.GetMouseButton(1))
        {
            // Õ”«» «·„”«›… «· Ì  Õ—ﬂÂ« «·„«Ê” »Ì‰ «·›—Ì„ «·”«»ﬁ Ê«·Õ«·Ì
            Vector3 delta = Input.mousePosition - lastMousePosition;

            //  ÕÊÌ· Õ—ﬂ… «·„«Ê” ·« Ã«Â«  «·ﬂ«„Ì—«
            Vector3 forward = transform.forward;
            forward.y = 0;
            Vector3 right = transform.right;

            //  Õ—Ìﬂ «·Âœ› (Target) »‰«¡ ⁄·Ï Õ—ﬂ… «·„«Ê”
            // ÷—»‰« X ›Ì Right Ê Y ›Ì Forward (·√‰ Õ—ﬂ… «·„«Ê” 2D)
            Vector3 moveDir = (right * delta.x + forward * delta.y) * mouseSensitivity;

            // ‰ÿ—Õ «·ﬁÌ„… ·ÌﬂÊ‰ «·”Õ» ÿ»Ì⁄Ì (Drag the ground)
            targetPosition -= moveDir * Time.deltaTime * 10f;

            lastMousePosition = Input.mousePosition;
        }
    }

    void HandleRotation()
    {
        if (Input.GetKey(KeyCode.Q)) currentYRotation -= rotationSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.E)) currentYRotation += rotationSpeed * Time.deltaTime;
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            targetPosition.y -= scroll * zoomSpeed * Time.deltaTime;
            targetPosition.y = Mathf.Clamp(targetPosition.y, minHeight, maxHeight);
        }
    }
}