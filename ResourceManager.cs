using UnityEngine;
using TMPro;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance;

    [Header("Resources")]
    public int currentGold = 100;
    public TextMeshProUGUI goldDisplayText;

    private void Awake()
    {
        // Singleton Setup
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        UpdateGoldUI();
    }

    public void AddGold(int amount)
    {
        currentGold += amount;
        UpdateGoldUI();
    }

    public bool TrySpendGold(int amount)
    {
        if (currentGold >= amount)
        {
            currentGold -= amount;
            UpdateGoldUI();
            return true;
        }
        return false;
    }

    public int GetCurrentGold()
    {
        return currentGold;
    }

    private void UpdateGoldUI()
    {
        if (goldDisplayText != null)
        {
            goldDisplayText.text = "Gold: " + currentGold;
        }
    }
}
