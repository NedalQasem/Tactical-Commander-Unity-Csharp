using UnityEngine;
using UnityEngine.UI;

public class ReturnToHQButton : MonoBehaviour
{
    [Header("Dependencies")]
    public CameraControl cameraControl;
    public Transform hqTransform;

    [Header("Settings")]
    public float showDistanceThreshold = 15f; // Min distance to show arrow
    public RectTransform arrowRect; // Assign the UI RectTransform here

    private Button button;

    void Start()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnClicked);
        }

        if (cameraControl == null) cameraControl = FindFirstObjectByType<CameraControl>();
        
        // Auto-find HQ if not assigned
        if (hqTransform == null)
        {
            Castle[] castles = FindObjectsByType<Castle>(FindObjectsSortMode.None);
            foreach (var c in castles)
            {
                if (c.TryGetComponent(out BuildingBase b) && b.team == Unit.Team.Player) 
                {
                    hqTransform = c.transform;
                    break;
                }
            }
        }
    }

    private bool isVisible = true; // Track state internally

    void LateUpdate()
    {
        if (hqTransform == null || cameraControl == null) return;

        // Check distance (Top-down X/Z)
        Vector3 camPos = cameraControl.transform.position;
        Vector3 hqPos = hqTransform.position;
        float dist = Vector2.Distance(new Vector2(camPos.x, camPos.z), new Vector2(hqPos.x, hqPos.z));
        
        bool shouldShow = dist > showDistanceThreshold;
        
        // Optimize: Only toggle when state changes
        if (isVisible != shouldShow)
        {
            isVisible = shouldShow;
            ToggleVisuals(isVisible);
        }

        if (isVisible && arrowRect != null)
        {
            // Point to HQ
            Vector3 dir = (hqPos - camPos).normalized;
            
            // Calculate Angle (Standard Grid: X=Right, Z=Up)
            float angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            
            // Apply rotation (UI Rotation is usually negative for Z-axis clock-wise from Up)
            arrowRect.localEulerAngles = new Vector3(0, 0, -angle);
        }
    }

    void ToggleVisuals(bool state)
    {
        // Toggle Images (Button Background, Icon)
        foreach(var img in GetComponentsInChildren<Image>(true)) img.enabled = state;
        
        // Toggle Text (if any)
        foreach(var txt in GetComponentsInChildren<Text>(true)) txt.enabled = state;
        
        // Disable Button Interactability
        if (button != null) button.interactable = state;
    }

    public void OnClicked() // Made public just in case user wants to link manually
    {
        if (cameraControl != null)
        {
            cameraControl.FocusOnCastle();
        }
    }
}
