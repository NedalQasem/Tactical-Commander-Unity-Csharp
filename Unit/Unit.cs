using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.UI;

public abstract class Unit : MonoBehaviour, IDamageable
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
    [HideInInspector] public UnitAnimation unitAnimation;

    [Header("Combat Settings")]
    public float visionRange = 10f;
    public IDamageable target;
    public UnitStateMachine stateMachine;

    public string unitName => data != null ? data.unitName : "Unknown Unit";
    public int maxHP => data != null ? data.maxHealth : 100;
    
    // 🛡️ Helper Property for Safe Agent Access
    public bool IsAgentReady => agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh;

    // --- Abstract Methods for Subclasses ---
    public abstract void TryAttack(IDamageable target);
    public abstract float GetAttackRange(IDamageable target);

    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        unitAnimation = GetComponent<UnitAnimation>();
        if (unitAnimation == null) unitAnimation = gameObject.AddComponent<UnitAnimation>();
        
        if (data != null)
        {
            currentHP = data.maxHealth;
            if (agent != null) agent.speed = data.moveSpeed;
        }

        if (selectionCircle != null) selectionCircle.SetActive(false);
        if (healthBarObject != null) healthBarObject.SetActive(false);
    }

    protected virtual IEnumerator Start()
    {
        // Wait one frame to ensure NavMeshAgent is placed on the NavMesh
        yield return null;

        if (data != null) currentHP = data.maxHealth;
        
        // Initialize State Machine safely
        stateMachine = new UnitStateMachine(this);
        stateMachine.Initialize(new UnitState_Idle());
    }

    protected virtual void Update()
    {
        if (stateMachine != null) stateMachine.Update();
    }

    // --- Selection Logic ---
    private bool isSelected = false;

    public void OnSelect()
    {
        isSelected = true;
        if (selectionCircle != null) selectionCircle.SetActive(true);
        UpdateHealthBarVisibility();
    }

    public void OnDeselect()
    {
        isSelected = false;
        if (selectionCircle != null) selectionCircle.SetActive(false);
        UpdateHealthBarVisibility();
    }

    public void SetSelected(bool selected)
    {
        if (selected) OnSelect();
        else OnDeselect();
    }

    void UpdateHealthBarVisibility()
    {
        if (healthBarObject != null)
        {
            bool shouldShow = isSelected || (currentHP < maxHP && currentHP > 0);
            healthBarObject.SetActive(shouldShow);
        }
    }

    // --- Commands ---
    public void MoveTo(Vector3 destination)
    {
        // 🛡️ Safety: Initialize if not ready
        if (stateMachine == null)
        {
            stateMachine = new UnitStateMachine(this);
            stateMachine.Initialize(new UnitState_Idle());
        }

        target = null; 
        stateMachine.ChangeState(new UnitState_Move(destination));
    }

    public void StopMoving()
    {
        if (IsAgentReady)
        {
            agent.ResetPath();
        }
    }

    // --- IDamageable Implementation ---
    public Unit.Team GetTeam() { return team; }
    public Transform GetTransform() { return transform; }
    public bool IsAlive() { return currentHP > 0; }
    public float GetRadius() { return agent != null ? agent.radius : 0.5f; }
    public Collider GetCollider() { return GetComponent<Collider>(); } // 🛡️ Simple implementation for Unit

    // 🎯 Auto-Targeting Helper
    public IDamageable ScanForEnemies(float range)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, range);
        foreach (var hit in hits)
        {
            // 🛡️ Use GetComponentInParent just in case collider is on a child mesh
            IDamageable d = hit.GetComponentInParent<IDamageable>();
            // 🛡️ Safety Check: Ensure the object isn't destroyed
            if ((d as UnityEngine.Object) != null && d != null && d.GetTeam() != team && d.IsAlive())
            {
                return d; // Found an enemy!
            }
        }
        return null; // No enemies in range
    }

    public void TakeDamage(int amount)
    {
        currentHP -= amount;
        UpdateHealthUI();
        UpdateHealthBarVisibility();
        if (currentHP <= 0) Die();
    }

    void UpdateHealthUI()
    {
        if (healthBarFill != null && data != null)
        {
            healthBarFill.fillAmount = currentHP / data.maxHealth;
        }
    }

    protected virtual void Die()
    {
        // Add death logic later (animation, pool return, etc)
        Destroy(gameObject);
    }

    // --- AI Helper ---
    public bool FindClosestEnemy()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, visionRange);
        IDamageable bestTarget = null;
        float closestDist = Mathf.Infinity;

        foreach (Collider hit in hits)
        {
            IDamageable potentialTarget = hit.GetComponentInParent<IDamageable>();
            
            // 🛡️ Safety Check: Ensure the object isn't destroyed
            if ((potentialTarget as UnityEngine.Object) != null && 
                potentialTarget != null && 
                potentialTarget.GetTeam() != this.team && 
                potentialTarget.IsAlive())
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

    // --- Gizmos for Debugging ---
    protected virtual void OnDrawGizmosSelected()
    {
        if (data != null)
        {
            // Attack Range (Red) - using property might not work in editor if not initialized, usage cautious
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, data.attackRange);
        }

        // Vision Range (Yellow)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        // Target Line (Blue)
        if (target != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, target.GetTransform().position);
        }
    }
}
