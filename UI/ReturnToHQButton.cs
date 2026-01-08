using UnityEngine;
using UnityEngine.UI;

public class ReturnToHQButton : MonoBehaviour
{
    [Header("Dependencies")]
    public CameraControl cameraControl;
    public Transform hqTransform;

    [Header("Settings")]
    public float showDistanceThreshold = 15f; 
    public RectTransform arrowRect;

    private Button button;

    void Start()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnClicked);
        }

        if (cameraControl == null) cameraControl = FindFirstObjectByType<CameraControl>();
        
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

        Vector3 camPos = cameraControl.transform.position;
        Vector3 hqPos = hqTransform.position;
        float dist = Vector2.Distance(new Vector2(camPos.x, camPos.z), new Vector2(hqPos.x, hqPos.z));
        
        bool shouldShow = dist > showDistanceThreshold;
        
        if (isVisible != shouldShow)
        {
            isVisible = shouldShow;
            ToggleVisuals(isVisible);
        }

        if (isVisible && arrowRect != null)
        {
            Vector3 dir = (hqPos - camPos).normalized;
            
            float angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            
            arrowRect.localEulerAngles = new Vector3(0, 0, -angle);
        }
    }

    void ToggleVisuals(bool state)
    {
        foreach(var img in GetComponentsInChildren<Image>(true)) img.enabled = state;
        
        foreach(var txt in GetComponentsInChildren<Text>(true)) txt.enabled = state;
        
        if (button != null) button.interactable = state;
    }

    public void OnClicked() 
    {
        if (cameraControl != null)
        {
            cameraControl.FocusOnCastle();
        }
    }
}
