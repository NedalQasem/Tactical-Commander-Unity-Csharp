using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class PlacementManager : MonoBehaviour
{
    [Header("References")]
    public GameObject soldierPrefab;
    public GameObject goldMinePrefab;
    public GameObject barracksPrefab;
    public Transform borderLine;
    public LayerMask groundMask;

    [Header("Rally Point")]
    public GameObject rallyPointParams; 

    [Header("Costs")]
    public int soldierCost = 10;
    public int mineCost = 50;
    public int barracksCost = 50;

    [Header("Building Limits")]
    public int maxMines = 3;
    private int currentMinesCount = 0;
    public int maxBarracks = 2;
    private int currentBarracksCount = 0;

    [Header("Ghost System")]
    public Material ghostBaseMaterial;
    private PlacementGhost activeGhost;

    public List<Unit> selectedUnits = new List<Unit>();

    public enum PlacementMode { None, Soldier, Mine, Barracks }
    private PlacementMode currentMode = PlacementMode.None;

    public enum FormationType { None, Single, Line, Square, Wedge }
    private FormationType currentFormation = FormationType.None;

    private float currentBuildingRotation = 0f;

    [Header("Selection & Info UI")]
    [Header("Selection & Info UI")]
    // public GameObject infoPanel; // Removed as requested
    public TextMeshProUGUI entityNameText;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI priceText;
    private BuildingBase selectedBuilding;

    [Header("Tooltip UI Settings")]
    public GameObject tooltipPanel;
    public TextMeshProUGUI tooltipNameText;
    public TextMeshProUGUI tooltipCostText;
    public float tooltipYOffset = 50f;

    // --- Box Selection Variables ---
    private Vector2 dragStartPos;
    private bool isDragging = false;
    private Texture2D whiteTexture; 

    private void Start() 
    {
        // Setup Box Texture
        whiteTexture = new Texture2D(1, 1);
        whiteTexture.SetPixel(0, 0, new Color(0, 1, 0, 0.2f));
        whiteTexture.Apply();
    }

    void Update()
    {
        if (activeGhost != null && Input.GetKeyDown(KeyCode.R))
        {
            currentBuildingRotation += 90f;
            if (currentBuildingRotation >= 360f) currentBuildingRotation = 0f;
            activeGhost.transform.rotation = Quaternion.Euler(0, currentBuildingRotation, 0);
        }

        if (activeGhost != null) HandlePlacementLogic();

        if (Input.GetKeyDown(KeyCode.Escape)) CancelPlacement();
    }

    // --- Selection Logic Moved to SelectionManager ---
    // We keep UI references here for now to avoid breaking references in Editor, 
    // but logic is exposed via public methods.

    public bool IsPlacingBuilding()
    {
        return activeGhost != null;
    }

    public void UpdateSelectionUI(BuildingBase building)
    {
        // Reuse existing SelectBuilding logic but public
        // if (infoPanel != null) infoPanel.SetActive(true); // Removed
        if (entityNameText != null) entityNameText.text = building.buildingName;
        if (healthText != null) healthText.text = $"HP: {building.currentHealth} / {building.maxHealth}";

        if (building is GoldMine mine)
        {
            if (priceText != null) priceText.text = $"Cost: {mine.constructionCost} | Yield: {mine.goldAmount} per {mine.interval}s";
            if (BarracksUIManager.Instance != null) BarracksUIManager.Instance.CloseBarracksUI();
        }
        else if (building is Barracks barracks)
        {
            if (priceText != null) priceText.text = "Function: Unit Production Center";
            if (BarracksUIManager.Instance != null) BarracksUIManager.Instance.OpenBarracksUI(barracks);
            if (barracks.visualRallyPoint != null) barracks.visualRallyPoint.SetActive(true);
        }
        else
        {
            if (priceText != null) priceText.text = "Construction Cost: " + building.constructionCost;
            if (BarracksUIManager.Instance != null) BarracksUIManager.Instance.CloseBarracksUI();
        }
    }

    public void ClearSelectionUI()
    {
        // if (infoPanel != null) infoPanel.SetActive(false); // Removed
        if (BarracksUIManager.Instance != null) BarracksUIManager.Instance.CloseBarracksUI();
    }

    // --- End Selection Logic ---

    void FinishPlacement(Vector3 pos)
    {
        if (currentMode == PlacementMode.Soldier) ExecuteSoldierPlacement(pos);
        else if (currentMode == PlacementMode.Mine) ExecuteMinePlacement(pos);
        else if (currentMode == PlacementMode.Barracks) ExecuteBarracksPlacement(pos);
        CancelPlacement();
    }

    public void TryPlaceMine()
    {
        if (currentMinesCount >= maxMines) return;
        CancelPlacement();
        
        // Use ResourceManager
        if (ResourceManager.Instance != null && ResourceManager.Instance.GetCurrentGold() < mineCost) return;

        currentMode = PlacementMode.Mine;
        GameObject ghostObj = Instantiate(goldMinePrefab);
        activeGhost = ghostObj.AddComponent<PlacementGhost>();
        activeGhost.Initialize(ghostBaseMaterial);
    }

    void ExecuteMinePlacement(Vector3 pos)
    {
        // Use ResourceManager
        if (ResourceManager.Instance != null && ResourceManager.Instance.TrySpendGold(mineCost))
        {
            GameObject newMine = Instantiate(goldMinePrefab, pos, Quaternion.Euler(0, currentBuildingRotation, 0));
            newMine.GetComponent<GoldMine>().isPlaced = true;
            currentMinesCount++;
        }
    }

    public void TryPlaceBarracks()
    {
        if (currentBarracksCount >= maxBarracks) return;
        CancelPlacement();

        // Use ResourceManager
        if (ResourceManager.Instance != null && ResourceManager.Instance.GetCurrentGold() < barracksCost) return;

        currentMode = PlacementMode.Barracks;
        GameObject ghostObj = Instantiate(barracksPrefab);
        activeGhost = ghostObj.AddComponent<PlacementGhost>();
        activeGhost.Initialize(ghostBaseMaterial);
    }

    void ExecuteBarracksPlacement(Vector3 pos)
    {
        // Use ResourceManager
        if (ResourceManager.Instance != null && ResourceManager.Instance.TrySpendGold(barracksCost))
        {
            GameObject newBarracks = Instantiate(barracksPrefab, pos, Quaternion.Euler(0, currentBuildingRotation, 0));
            Barracks barracksScript = newBarracks.GetComponent<Barracks>();
            if (barracksScript != null) barracksScript.isPlaced = true;
            currentBarracksCount++;
        }
    }

    public void SetFormation(int index)
    {
        CancelPlacement();
        currentFormation = (FormationType)(index + 1);
        currentMode = PlacementMode.Soldier;
        GameObject ghostObj = new GameObject("FormationGhost");
        activeGhost = ghostObj.AddComponent<FormationGhost>();
        ((FormationGhost)activeGhost).Setup(soldierPrefab, GetFormationOffsets(), ghostBaseMaterial);
    }

    void ExecuteSoldierPlacement(Vector3 pos)
    {
        List<Vector3> offsets = GetFormationOffsets();
        int totalCost = offsets.Count * soldierCost;
        
        // Use ResourceManager
        if (ResourceManager.Instance != null && ResourceManager.Instance.GetCurrentGold() >= totalCost)
        {
            ResourceManager.Instance.TrySpendGold(totalCost);
            foreach (var offset in offsets)
            {
                Instantiate(soldierPrefab, pos + offset, Quaternion.identity);
            }
        }
    }

    public void ShowTooltip(string bName, int bCost, RectTransform buttonRect)
    {
        // إذا كان هناك مبنى محدد، لا تظهر التلميح
        if (selectedBuilding != null) return;
        
        tooltipPanel.SetActive(true);
        tooltipNameText.text = bName;
        tooltipCostText.text = "Cost: " + bCost;

        // حساب الموضع الجديد: مركز الزر + نصف ارتفاع الزر + الإزاحة المطلوبة
        // هذا يضمن ظهور التلميح فوق الزر تماماً بغض النظر عن حجم الزر
        float halfHeight = (buttonRect.rect.height * buttonRect.lossyScale.y) / 2f;
        Vector3 newPos = buttonRect.position + new Vector3(50, halfHeight + tooltipYOffset, 0);
        
        tooltipPanel.transform.position = newPos;
    }

    public void HideTooltip() => tooltipPanel.SetActive(false);

    void CancelPlacement()
    {
        if (activeGhost != null) Destroy(activeGhost.gameObject);
        activeGhost = null;
        currentMode = PlacementMode.None;
        currentBuildingRotation = 0f;
    }

    List<Vector3> GetFormationOffsets()
    {
        List<Vector3> offsets = new List<Vector3>();
        switch (currentFormation)
        {
            case FormationType.Single: offsets.Add(Vector3.zero); break;
            case FormationType.Line: for (int i = 0; i < 5; i++) offsets.Add(new Vector3(0, 0, i * 1.5f - 3f)); break;
            case FormationType.Square: for (int x = 0; x < 3; x++) for (int z = 0; z < 3; z++) offsets.Add(new Vector3(x * 1.5f, 0, z * 1.5f)); break;
            case FormationType.Wedge:
                offsets.Add(Vector3.zero);
                offsets.Add(new Vector3(-1.5f, 0, 1.5f)); offsets.Add(new Vector3(-1.5f, 0, -1.5f));
                offsets.Add(new Vector3(-3f, 0, 3f)); offsets.Add(new Vector3(-3f, 0, -3f));
                break;
        }
        return offsets;
    }
    void HandlePlacementLogic()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundMask))
        {
            activeGhost.transform.position = hit.point;
            bool isInsideBorder = hit.point.x < borderLine.position.x;
            bool noOverlap = activeGhost.CanPlace();
            bool finalValid = isInsideBorder && noOverlap;
            activeGhost.SetColor(finalValid);

            if (Input.GetMouseButtonDown(0) && finalValid && !EventSystem.current.IsPointerOverGameObject())
            {
                FinishPlacement(hit.point);
            }
        }
    }
}
