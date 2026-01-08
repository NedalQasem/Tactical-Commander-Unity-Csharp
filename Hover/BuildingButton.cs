using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string bName;
    public int bCost;
    public string bDescription;

    private PlacementManager pManager;
    private RectTransform rectTransform;
    void Start()
    {
        pManager = FindFirstObjectByType<PlacementManager>();
        rectTransform = GetComponent<RectTransform>(); 
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        pManager.ShowTooltip(bName, bCost, GetComponent<RectTransform>());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        pManager.HideTooltip();
    }
}