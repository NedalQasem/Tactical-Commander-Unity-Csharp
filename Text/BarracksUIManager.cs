using UnityEngine;
using TMPro;

public class BarracksUIManager : MonoBehaviour
{
    public static BarracksUIManager Instance;
    public GameObject barracksUI; // RightPanel

    [Header("Stats Panel")]
    public GameObject statsPanel;
    public TextMeshProUGUI unitNameText;
    public TextMeshProUGUI unitStatsText; // HP & Damage
    public TextMeshProUGUI trainingTimeText;
    public TextMeshProUGUI costText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        if (barracksUI != null) barracksUI.SetActive(false);
        if (statsPanel != null) statsPanel.SetActive(false); // Hide by default
    }

    public void OpenBarracksUI(Barracks barracks)
    {
        if (barracksUI != null)
        {
            barracksUI.SetActive(true);
            UnitProductionButton[] buttons = barracksUI.GetComponentsInChildren<UnitProductionButton>(true);
            foreach (var btn in buttons)
            {
                btn.Initialize(barracks);
            }
        }
    }

    public void CloseBarracksUI()
    {
        if (barracksUI != null) barracksUI.SetActive(false);
        HideUnitStats(); // Also hide stats if closed
    }

    public void ShowUnitStats(UnitData data)
    {
        if (statsPanel != null)
        {
            statsPanel.SetActive(true);
            if (unitNameText != null) unitNameText.text = data.unitName;
            if (unitStatsText != null) unitStatsText.text = $"HP: {data.maxHealth} | DMG: {data.attackDamage}";
            if (trainingTimeText != null) trainingTimeText.text = $"Time: {data.trainingTime}s";
            if (costText != null) costText.text = $"Cost: {data.goldCost}";
        }
    }

    public void HideUnitStats()
    {
        if (statsPanel != null) statsPanel.SetActive(false);
    }
}
