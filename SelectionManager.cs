using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectionManager : MonoBehaviour
{
    public static SelectionManager Instance;

    [Header("Settings")]
    public LayerMask groundMask;
    public GameObject rallyPointPrefab; // ⬅️ المرجع المباشر الجديد

    // القوائم المختارة
    public List<Unit> selectedUnits = new List<Unit>();
    public BuildingBase selectedBuilding;

    // متغيرات الصندوق (Box Selection)
    private Vector2 dragStartPos;
    private bool isDragging = false;
    private Texture2D whiteTexture;

    // مراجع خارجية
    private PlacementManager placementManager;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // إعداد تكستشر المربع
        whiteTexture = new Texture2D(1, 1);
        whiteTexture.SetPixel(0, 0, new Color(0, 1, 0, 0.2f));
        whiteTexture.Apply();
    }

    void Start()
    {
        // استخدام FindFirstObjectByType للنسخ الحديثة من Unity
        placementManager = FindFirstObjectByType<PlacementManager>();
    }

    void Update()
    {
        // 1. إذا كنا في وضع البناء (يوجد شبح مبنى)، لا تقم بالتحديد
        if (placementManager != null && placementManager.IsPlacingBuilding()) return;

        // 2. إذا كان الماوس فوق UI، لا تقم بالتحديد
        if (EventSystem.current.IsPointerOverGameObject()) return;

        HandleInput();
    }

    void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            dragStartPos = Input.mousePosition;
            isDragging = true;
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            if (Vector2.Distance(dragStartPos, Input.mousePosition) < 10)
                HandleSingleClick();
            else
                HandleBoxSelection();
        }

        if (Input.GetMouseButtonDown(1))
        {
            HandleRightClick();
        }
    }

    void HandleSingleClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Unit unit = hit.collider.GetComponentInParent<Unit>();
            BuildingBase building = hit.collider.GetComponentInParent<BuildingBase>();

            if (unit != null)
            {
                if (!Input.GetKey(KeyCode.LeftShift)) DeselectAll();
                SelectUnit(unit);
            }
            else if (building != null)
            {
                DeselectAll();
                SelectBuilding(building);
            }
            else
            {
                if (!Input.GetKey(KeyCode.LeftShift)) DeselectAll();
            }
        }
    }

    void HandleBoxSelection()
    {
        Rect selectionRect = GetScreenRect(dragStartPos, Input.mousePosition);
        if (!Input.GetKey(KeyCode.LeftShift)) DeselectAll();
        
        Unit[] allUnits = FindObjectsByType<Unit>(FindObjectsSortMode.None);
        foreach (Unit unit in allUnits)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(unit.transform.position);
            // شرط الفريق: نختار فقط جنودنا في الصندوق
            if (screenPos.z > 0 && selectionRect.Contains(screenPos) && unit.team == Unit.Team.Player)
            {
                SelectUnit(unit);
            }
        }
    }

    void HandleRightClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f)) // Removed specific groundMask constraint for robustness
        {
            selectedUnits.RemoveAll(u => u == null);

            if (selectedUnits.Count > 0)
            {
                if (selectedUnits[0].team != Unit.Team.Player) return;

                IDamageable target = hit.collider.GetComponentInParent<IDamageable>();
                if (target != null && target.GetTeam() != Unit.Team.Player && target.IsAlive())
                {
                    foreach (Unit u in selectedUnits)
                    {
                        if (u != null)
                        {
                            u.target = target;
                            u.stateMachine.ChangeState(new UnitState_Chase(target));
                        }
                    }
                    return; 
                }

                selectedUnits.Sort((a, b) => {
                    int priorityA = GetUnitPriority(a.data.unitName);
                    int priorityB = GetUnitPriority(b.data.unitName);
                    return priorityA.CompareTo(priorityB);
                });

                Vector3 groupCenter = Vector3.zero;
                foreach (Unit u in selectedUnits) groupCenter += u.transform.position;
                groupCenter /= selectedUnits.Count;

                Vector3 direction = (hit.point - groupCenter).normalized;
                if (direction == Vector3.zero) direction = Vector3.forward;
                Quaternion rotation = Quaternion.LookRotation(direction);

                if (FormationManager.Instance != null)
                {
                    List<Vector3> targetPositions = FormationManager.Instance.GetFormationPositions(hit.point, selectedUnits.Count, rotation);
                    for (int i = 0; i < selectedUnits.Count; i++)
                    {
                        if (i < targetPositions.Count) selectedUnits[i].MoveTo(targetPositions[i]);
                    }
                }
                else
                {
                    foreach (Unit u in selectedUnits) u.MoveTo(hit.point);
                }
            }
            else if (selectedBuilding != null && selectedBuilding is Barracks barracks)
            {
                 if (barracks.rallyPoint != null) 
                 {
                     barracks.rallyPoint.position = hit.point;
                 }
                 
                 if (barracks.visualRallyPoint != null)
                 {
                     barracks.visualRallyPoint.transform.position = hit.point + Vector3.up * 1.0f;
                     if (!barracks.visualRallyPoint.activeSelf) barracks.visualRallyPoint.SetActive(true);
                     
                     if (barracks.rallyPoint == null) barracks.rallyPoint = barracks.visualRallyPoint.transform;
                 }
                 else if (rallyPointPrefab != null)
                 {
                      GameObject newRallyPoint = Instantiate(rallyPointPrefab, hit.point + Vector3.up * 0.5f, Quaternion.identity);
                      newRallyPoint.transform.localScale = Vector3.one; 
                      newRallyPoint.SetActive(true);
                      
                      var floater = newRallyPoint.GetComponent<FloatingText>();
                      if (floater != null) Destroy(floater);
                      
                      barracks.visualRallyPoint = newRallyPoint;
                      
                      barracks.rallyPoint = newRallyPoint.transform;
                 }
            }
        }
    }

    // --- Helpers ---

    int GetUnitPriority(string unitName)
    {
        // الأرقام الأقل تعني أولوية أعلى (في المقدمة)
        if (unitName.Contains("Melee")) return 1;
        if (unitName.Contains("Sword")) return 2;
        if (unitName.Contains("Archer")) return 3;
        return 99; // غير معروف، يوضع في الخلف
    }

    void SelectUnit(Unit unit)
    {
        if (!selectedUnits.Contains(unit))
        {
            selectedUnits.Add(unit);
            unit.OnSelect();
        }
        if (selectedBuilding != null) DeselectBuilding();
    }

    void SelectBuilding(BuildingBase building)
    {
        selectedBuilding = building;
        
        building.isSelected = true;
        building.UpdateHealthBarVisibility();
        
        // 🔒 Security: Only show Control UI for Player buildings
        if (placementManager != null)
        {
            if (building.team == Unit.Team.Player)
            {
                placementManager.UpdateSelectionUI(building);
            }
            else
            {
                // Inspecting enemy building? ensuring no buttons are shown
                placementManager.ClearSelectionUI();
            }
        }
    }

    public void DeselectAll()
    {
        foreach (Unit u in selectedUnits) if (u != null) u.OnDeselect();
        selectedUnits.Clear();
        DeselectBuilding();
    }

    void DeselectBuilding()
    {
        if (selectedBuilding != null)
        {
            selectedBuilding.isSelected = false;
            selectedBuilding.UpdateHealthBarVisibility();

            if (selectedBuilding is Barracks barracks && barracks.visualRallyPoint != null)
            {
                barracks.visualRallyPoint.SetActive(false);
            }
        }
        selectedBuilding = null;
        if (placementManager != null) placementManager.ClearSelectionUI();
    }

    Rect GetScreenRect(Vector2 pos1, Vector2 pos2)
    {
        Vector2 min = Vector2.Min(pos1, pos2);
        Vector2 max = Vector2.Max(pos1, pos2);
        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }

    void OnGUI()
    {
        if (isDragging)
        {
            Rect rect = GetScreenRect(dragStartPos, Input.mousePosition);
            Rect guiRect = new Rect(rect.x, Screen.height - rect.yMax, rect.width, rect.height);
            GUI.color = new Color(0, 1, 0, 0.3f);
            GUI.DrawTexture(guiRect, whiteTexture);
            GUI.color = Color.green;
            GUI.Box(guiRect, "");
            GUI.color = Color.white;
        }
    }
}
