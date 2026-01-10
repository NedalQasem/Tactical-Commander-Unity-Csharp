using UnityEngine;
using TMPro;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance; // Player's Resource Manager

    [Header("Settings")]
    public Unit.Team team = Unit.Team.Player;

    [Header("Resources")]
    [SerializeField] private int currentGold = 100;
    public TextMeshProUGUI goldDisplayText;

    private void Awake()
    {
        // Auto-detect if attached to Enemy AI
        if (GetComponent<EnemyAIController>() != null)
        {
            team = Unit.Team.Enemy;
        }

        // Singleton Setup ONLY for Player
        if (team == Unit.Team.Player)
        {
            if (Instance == null) Instance = this;
            else 
            {
                Debug.LogWarning("Duplicate Player ResourceManager found. Destroying.");
                Destroy(gameObject);
                return;
            }
        }
        
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
        // Only update UI for the Player
        if (team == Unit.Team.Player && goldDisplayText != null)
        {
            goldDisplayText.text = "Gold: " + currentGold;
        }
    }
}

