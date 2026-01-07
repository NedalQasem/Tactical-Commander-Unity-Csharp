using UnityEngine;
using UnityEngine.UI; // ضروري جداً للتعامل مع الصور

public abstract class BuildingBase : MonoBehaviour, IDamageable
{
    public int maxHealth = 100;
    public int currentHealth;
    public Unit.Team team = Unit.Team.Player; // ⬅️ إضافة الفريق

    public bool isSelected;
    [Header("UI References")]
    public Image healthBarFill;
    public string buildingName;
    public int constructionCost;
    
    [HideInInspector] public bool isPlaced = false;

    // --- IDamageable Implementation ---
    public Unit.Team GetTeam() { return team; }
    public Transform GetTransform() { return transform; }
    public bool IsAlive() { return currentHealth > 0; }
    public float GetRadius() { return 2.0f; } // قيمة تقريبية لحجم المبنى

    void Update()
    {
        if (!isPlaced) return;
    }

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
    }

    public virtual void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = (float)currentHealth / maxHealth;
        }
        if (currentHealth <= 0) Die();
    }

    protected virtual void Die() => Destroy(gameObject);
}