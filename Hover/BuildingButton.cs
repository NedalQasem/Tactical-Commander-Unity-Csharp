using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string bName;
    public int bCost;
    public string bDescription;

    private PlacementManager pManager;
    private RectTransform rectTransform; // áÊÎÒíä ãæŞÚ ÇáÒÑ
    void Start()
    {
        pManager = FindFirstObjectByType<PlacementManager>();
        rectTransform = GetComponent<RectTransform>(); // ÇáÍÕæá Úáì ãßæä ÇáãæŞÚ
    }

    // íÊã ÇÓÊÏÚÇÄåÇ ÚäÏ ÏÎæá ÇáãÇæÓ ãäØŞÉ ÇáÒÑ
    public void OnPointerEnter(PointerEventData eventData)
    {
        // äÑÓá ÇáÇÓã¡ ÇáÓÚÑ¡ æÇáãæŞÚ (GetComponent<RectTransform>())
        pManager.ShowTooltip(bName, bCost, GetComponent<RectTransform>());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        pManager.HideTooltip();
    }
}