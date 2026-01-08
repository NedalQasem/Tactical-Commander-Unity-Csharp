using UnityEngine;

public class UnitState_Chase : IUnitState
{
    private Unit unit;
    private IDamageable target;
    private float updateTimer = 0f;

    public UnitState_Chase(IDamageable target)
    {
        this.target = target;
    }

    public void Enter(Unit unit)
    {
        this.unit = unit;
        if (target != null && unit.IsAgentReady)
        {
            unit.agent.isStopped = false;
            unit.agent.stoppingDistance = 0f; // 🛑 Force stop manual control
            unit.agent.SetDestination(target.GetTransform().position);
        }
    }

    public void Update(Unit unit)
    {
        // 1. Validate Target
        if (target == null || !target.IsAlive())
        {
            unit.target = null;
            unit.stateMachine.ChangeState(new UnitState_Idle());
            return;
        }

        // 2. Refresh Path periodically
        updateTimer += Time.deltaTime;
        if (updateTimer > 0.2f)
        {
            if (unit.IsAgentReady) unit.agent.SetDestination(target.GetTransform().position);
            updateTimer = 0f;
        }

        // 3. Check Distance using Abstract Method
        // This works for both Melee (Physical Contact) and Ranged (Attack Range)
        float goalDist = unit.GetAttackRange(target);
        
        // Calculate Distance
        // Note: For Melee, GetAttackRange includes Collider sizes, so we need consistent distance check
        // Ideally, we use Surface Distance for everything to be accurate
        float currentDist = GetSurfaceDistance(unit, target);

        if (currentDist <= goalDist)
        {
            unit.stateMachine.ChangeState(new UnitState_Attack(target));
        }
        else
        {
             // Keep moving
             if (unit.IsAgentReady) unit.agent.isStopped = false;
        }
    }

    public void Exit(Unit unit)
    {
        if (unit.IsAgentReady) unit.agent.isStopped = true;
    }

    private float GetSurfaceDistance(Unit unit, IDamageable target)
    {
        // If MeleeUnit uses surface logic, we should use it here too
        Collider targetCol = target.GetCollider();
        if (targetCol != null)
        {
            Vector3 closestPoint = targetCol.ClosestPoint(unit.transform.position);
            return Vector3.Distance(unit.transform.position, closestPoint);
        }
        
        // Fallback for objects without cached colliders
        return Vector3.Distance(unit.transform.position, target.GetTransform().position) - target.GetRadius();
    }
}

