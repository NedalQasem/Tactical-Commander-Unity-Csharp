using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Unit : MonoBehaviour, IDamageable
{
    [Header("Data Reference")]
    public UnitData data;
    public enum Team { Player, Enemy }
    public Team team = Team.Player;

    [Header("UI & Visuals")]
    public GameObject selectionCircle;
    public GameObject healthBarObject;
    public Image healthBarFill;

    // Runtime Stats
    [HideInInspector] public float currentHP;
    [HideInInspector] public NavMeshAgent agent;

    public string unitName => data != null ? data.unitName : "Unknown Unit";
    public int maxHP => data != null ? data.maxHealth : 100;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (data != null)
        {
            currentHP = data.maxHealth;
            if (agent != null) agent.speed = data.moveSpeed;
        }
        else
        {
            currentHP = 100;
        }

        if (selectionCircle != null) selectionCircle.SetActive(false);
        if (healthBarObject != null) healthBarObject.SetActive(false);

        // Initialize State Machine here to be ready for immediate commands
        stateMachine = new UnitStateMachine(this);
        stateMachine.Initialize(new UnitState_Idle());
    }

    public void OnSelect()
    {
        if (selectionCircle != null) selectionCircle.SetActive(true);
        if (healthBarObject != null) healthBarObject.SetActive(true);
    }

    public void OnDeselect()
    {
        if (selectionCircle != null) selectionCircle.SetActive(false);
        if (healthBarObject != null) healthBarObject.SetActive(false);
    }

    public void SetSelected(bool selected)
    {
        if (selected) OnSelect();
        else OnDeselect();
    }

    // State Machine
    public UnitStateMachine stateMachine;
    
    // Combat
    public IDamageable target;
    public float visionRange = 10f; // مدى الرؤية (أوسع من مدى الهجوم)

    private void Update()
    {
        stateMachine.Update();
    }

    public void MoveTo(Vector3 destination)
    {
        // 🛡️ حماية: إذا لم يتم تجهيز الـ AI بعد، جهزه فوراً
        if (stateMachine == null)
        {
            stateMachine = new UnitStateMachine(this);
            stateMachine.Initialize(new UnitState_Idle());
        }

        target = null; 
        stateMachine.ChangeState(new UnitState_Move(destination));
    }

    // --- IDamageable Implementation ---
    public Unit.Team GetTeam() { return team; }
    public Transform GetTransform() { return transform; }
    public bool IsAlive() { return currentHP > 0; }
    public float GetRadius() { return agent != null ? agent.radius : 0.5f; }

    // --- AI Logic ---
    public bool FindClosestEnemy()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, visionRange);
        IDamageable bestTarget = null;
        float closestDist = Mathf.Infinity;

        foreach (Collider hit in hits)
        {
            IDamageable potentialTarget = hit.GetComponent<IDamageable>();
            
            if (potentialTarget != null && potentialTarget.GetTeam() != this.team && potentialTarget.IsAlive())
            {
                float dist = Vector3.Distance(transform.position, potentialTarget.GetTransform().position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    bestTarget = potentialTarget;
                }
            }
        }

        if (bestTarget != null)
        {
            target = bestTarget;
            return true;
        }
        return false;
    }

    // Future methods for State Machine
    public void StopMoving()
    {
        if (agent != null && agent.isOnNavMesh)
        {
            agent.ResetPath();
        }
    }

    public void TakeDamage(int amount)
    {
        currentHP -= amount;
        UpdateHealthUI();
        if (currentHP <= 0) Die();
    }

    void UpdateHealthUI()
    {
        if (healthBarFill != null && data != null)
        {
            healthBarFill.fillAmount = currentHP / data.maxHealth;
        }
    }

    void Die()
    {
        // Add death logic later (animation, pool return, etc)
        Destroy(gameObject);
    
    }
}
