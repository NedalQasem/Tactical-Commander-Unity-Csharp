using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 20f;
    public float zoomSpeed = 2000f; // Adjusted for Time.deltaTime
    public float rotateSpeed = 100f;

    [Header("Zoom Limits")]
    public float minZoom = 5f;
    public float maxZoom = 25f;

    [Header("Map Boundaries (Clamping)")]
    public bool useBoundaries = true;
    public float minX = -50f;
    public float maxX = 50f;
    public float minZ = -50f;
    public float maxZ = 50f;

    [Header("Edge Scrolling")]
    public bool useEdgeScrolling = false;
    public float edgeBorderThickness = 10f;

    void Start()
    {
        FocusOnCastle();
    }

    public void FocusOnCastle()
    {
        Vector3 targetPos = Vector3.zero;
        bool found = false;

        // Strategy 1: Look for "Castle" script
        Castle[] castles = FindObjectsByType<Castle>(FindObjectsSortMode.None);
        foreach (var c in castles)
        {
            if (c.TryGetComponent(out BuildingBase b) && b.team == Unit.Team.Player) 
            {
                targetPos = c.transform.position;
                found = true;
                break;
            }
        }

        // Strategy 2: Look for GameObject Name containing "Castle" or "Headquarters" & Player Team
        if (!found)
        {
            BuildingBase[] buildings = FindObjectsByType<BuildingBase>(FindObjectsSortMode.None);
            foreach (var b in buildings)
            {
                if (b.team == Unit.Team.Player && (b.name.Contains("Castle") || b.name.Contains("HQ") || b.name.Contains("Main")))
                {
                    targetPos = b.transform.position;
                    found = true;
                    Debug.Log($"Camera: Found HQ by name: {b.name}");
                    break;
                }
            }
        }

        // Strategy 3: Just find ANY Player Building if we are desperate
        if (!found)
        {
             BuildingBase[] buildings = FindObjectsByType<BuildingBase>(FindObjectsSortMode.None);
             foreach (var b in buildings)
             {
                 if (b.team == Unit.Team.Player)
                 {
                     targetPos = b.transform.position;
                     found = true;
                     break;
                 }
             }
        }

        if (found)
        {
            // Apply Offset
            targetPos.y = transform.position.y;
            targetPos.z -= 15f; // Pull back slightly more
            targetPos.x -= 0f;  // Keep X centered
            
            // Clamp to boundaries if valid
            if (useBoundaries)
            {
                targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
                targetPos.z = Mathf.Clamp(targetPos.z, minZ, maxZ);
            }

            transform.position = targetPos;
        }
        else
        {
            Debug.LogWarning("CameraControl: Could not find Player Castle or HQ!");
        }
    }

    void Update()
    {
        HandleMovement();
        HandleZoom();
    }

    private Vector3 lastMousePos;
    public float dragSpeed = 2f;

    void HandleMovement()
    {
        Vector3 pos = transform.position;
        Vector3 moveDir = Vector3.zero;

        // --- 1. Rotation (Q/E) ---
        if (Input.GetKey(KeyCode.Q)) transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);
        if (Input.GetKey(KeyCode.E)) transform.Rotate(Vector3.up, -rotateSpeed * Time.deltaTime, Space.World);

        // --- 2. Keyboard Movement (WASD / Arrows) ---
        // Calculate Forward/Right vectors relative to Camera Rotation (ignoring Y tilt)
        Vector3 forward = transform.forward;
        forward.y = 0;
        forward.Normalize();
        Vector3 right = transform.right;
        right.y = 0;
        right.Normalize();

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) moveDir += forward;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) moveDir -= forward;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) moveDir -= right;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) moveDir += right;

        // --- 3. Edge Scrolling ---
        if (useEdgeScrolling)
        {
            if (Input.mousePosition.y >= Screen.height - edgeBorderThickness) moveDir += forward;
            if (Input.mousePosition.y <= edgeBorderThickness) moveDir -= forward;
            if (Input.mousePosition.x <= edgeBorderThickness) moveDir -= right;
            if (Input.mousePosition.x >= Screen.width - edgeBorderThickness) moveDir += right;
        }

        // Apply Keyboard/Edge Velocity
        pos += moveDir.normalized * moveSpeed * Time.deltaTime;

        // --- 4. Mouse Drag Panning (Right or Middle) ---
        if (Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
        {
            lastMousePos = Input.mousePosition;
        }

        if (Input.GetMouseButton(1) || Input.GetMouseButton(2))
        {
            // Calculate screen delta
            Vector3 delta = Input.mousePosition - lastMousePos;
            
            // Map Screen Movement to World Plane
            // Dragging LEFT (Negative X) should move camera RIGHT (Positive X) to "pull" the world
            // Dragging UP (Positive Y) should move camera DOWN (Negative Z)
            
            // Sensitivity Factor
            float speed = dragSpeed * 0.05f * (transform.position.y / 10f); // Scale with Zoom height

            Vector3 dragMove = -right * delta.x * speed;
            dragMove += -forward * delta.y * speed;

            pos += dragMove;
            lastMousePos = Input.mousePosition;
        }

        // 🔒 5. Clamping
        if (useBoundaries)
        {
            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            pos.z = Mathf.Clamp(pos.z, minZ, maxZ);
        }

        transform.position = pos;
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        Vector3 pos = transform.position;

        // Move Y position for zoom
        pos.y -= scroll * zoomSpeed * Time.deltaTime;
        pos.y = Mathf.Clamp(pos.y, minZoom, maxZoom);

        transform.position = pos;
    }
}
