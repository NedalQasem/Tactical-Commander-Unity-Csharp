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
        // ⚠️ Fix: Same safety check for Enter logic
        if (target != null && (target as UnityEngine.Object) != null && unit.IsAgentReady)
        {
            unit.agent.isStopped = false;
            unit.agent.stoppingDistance = 0f; // 🛑 Force stop manual control
            unit.agent.SetDestination(target.GetTransform().position);
        }
    }

    public void Update(Unit unit)
    {
        if (target == null || (target as UnityEngine.Object) == null || !target.IsAlive())
        {
            unit.target = null;
            unit.stateMachine.ChangeState(new UnitState_Idle());
            return;
        }

        updateTimer += Time.deltaTime;
        if (updateTimer > 0.2f)
        {
            if (unit.IsAgentReady) unit.agent.SetDestination(target.GetTransform().position);
            updateTimer = 0f;
        }

        float goalDist = unit.GetAttackRange(target);
        
        float currentDist = GetSurfaceDistance(unit, target);

        if (currentDist <= goalDist)
        {
            unit.stateMachine.ChangeState(new UnitState_Attack(target));
        }
        else
        {
             if (unit.IsAgentReady) unit.agent.isStopped = false;
        }
    }

    public void Exit(Unit unit)
    {
        if (unit.IsAgentReady) unit.agent.isStopped = true;
    }

    private float GetSurfaceDistance(Unit unit, IDamageable target)
    {
        Collider targetCol = target.GetCollider();
        if (targetCol != null)
        {
            Vector3 closestPoint = targetCol.ClosestPoint(unit.transform.position);
            return Vector3.Distance(unit.transform.position, closestPoint);
        }
        
        return Vector3.Distance(unit.transform.position, target.GetTransform().position) - target.GetRadius();
    }
}

