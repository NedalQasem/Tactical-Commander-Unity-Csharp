using UnityEngine;
using UnityEngine.UI; // ضروري جداً للتعامل مع الصور

public abstract class BuildingBase : MonoBehaviour, IDamageable
{
    public int maxHealth = 100;
    public int currentHealth;
    public Unit.Team team = Unit.Team.Player; // ⬅️ إضافة الفريق

    public bool isSelected;
    [Header("UI References")]
    public GameObject healthBarContainer; // ⬅️ الحاوية
    public Image healthBarFill;
    public string buildingName;
    public int constructionCost;
    
    [HideInInspector] public bool isPlaced = false;

    // --- IDamageable Implementation ---
    public Unit.Team GetTeam() { return team; }
    public Transform GetTransform() { return transform; }
    public bool IsAlive() { return currentHealth > 0; }
    public float GetRadius() { return 2.0f; } // قيمة تقريبية لحجم المبنى
    public Collider GetCollider() { return _collider; } // 🛡️ Return cached collider

    protected Collider _collider;

    void Update()
    {
        if (!isPlaced) return;
    }

    protected virtual void Awake()
    {
        _collider = GetComponent<Collider>(); // 🛡️ Cache Cache Cache
        currentHealth = maxHealth;
        UpdateHealthBarVisibility();
    }

    public virtual void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = (float)currentHealth / maxHealth;
        }
        UpdateHealthBarVisibility();
        if (currentHealth <= 0) Die();
    }
    
    public void UpdateHealthBarVisibility()
    {
        if (healthBarContainer != null)
        {
            bool shouldShow = isSelected || (currentHealth < maxHealth && currentHealth > 0);
            healthBarContainer.SetActive(shouldShow);
        }
    }

    protected virtual void Die()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFXAt(SoundType.BuildingDestroyed, transform.position);
        Destroy(gameObject);
    }
}