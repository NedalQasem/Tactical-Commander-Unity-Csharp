using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class UnitProductionButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Data")]
    public UnitData unitData;

    [Header("Visual Elements")]
    public Image cooldownImage;
    public TextMeshProUGUI queueText;

    private Barracks linkedBarracks;

    public void Initialize(Barracks barracks)
    {
        linkedBarracks = barracks;
    }

    void Update()
    {
        if (linkedBarracks == null || unitData == null) return;

        if (queueText != null)
        {
            int count = linkedBarracks.GetQueueCount(unitData);
            queueText.text = count > 0 ? count.ToString() : "";
        }

        if (cooldownImage != null)
        {
            float progress = linkedBarracks.GetTrainingProgress(unitData);
            cooldownImage.fillAmount = progress;
        }
    }

    public void OnButtonClick()
    {
        if (linkedBarracks != null && ResourceManager.Instance != null)
        {
            if (ResourceManager.Instance.TrySpendGold(unitData.goldCost))
            {
                linkedBarracks.EnqueueUnit(unitData);
            }
            else
            {
                Debug.Log("Not enough gold!");
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (BarracksUIManager.Instance != null && unitData != null)
        {
            BarracksUIManager.Instance.ShowUnitStats(unitData);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (BarracksUIManager.Instance != null)
        {
            BarracksUIManager.Instance.HideUnitStats();
        }
    }
}
