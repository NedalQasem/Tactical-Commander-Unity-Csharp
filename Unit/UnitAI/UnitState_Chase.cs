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

        // 3. Logic Split: Melee vs Ranged
        float attackRange = (unit.data != null) ? unit.data.attackRange : 2.0f;
        bool isMelee = (unit.data != null && unit.data.attackType == UnitData.AttackType.Melee);

        // Calculate Surface Distance
        float dist;
        Collider targetCol = target.GetCollider();
        if (targetCol != null)
        {
            Vector3 closestPoint = targetCol.ClosestPoint(unit.transform.position);
            dist = Vector3.Distance(unit.transform.position, closestPoint);
        }
        else
        {
            dist = Vector3.Distance(unit.transform.position, target.GetTransform().position);
            dist -= target.GetRadius();
        }

        // 🛑 Decide Goal Distance
        float goalDist;
        if (isMelee)
        {
            // Melee: ZERO distance. Touch the collider.
            goalDist = 0.2f; // Small epsilon for float errors
        }
        else
        {
            // Ranged: Respect attack range
            goalDist = attackRange;
        }

        if (dist <= goalDist)
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
}
