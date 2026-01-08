using UnityEngine;

public class UnitState_Attack : IUnitState
{
    private Unit unit;
    private IDamageable target;
    private float attackTimer = 0f;

    public UnitState_Attack(IDamageable target)
    {
        this.target = target;
    }

    public void Enter(Unit unit)
    {
        this.unit = unit;
        if (unit.IsAgentReady) unit.agent.isStopped = true; 
    }

    public void Update(Unit unit)
    {
        // 🛡️ Robust Target Validation
        // We cast to UnityEngine.Object to ensure we catch destroyed objects correctly
        if ((target as UnityEngine.Object) == null || !target.IsAlive())
        {
            // 🎯 Target Switching: Look for next enemy
            // Use the unit's inspector vision range to be consistent with Idle state
            float visionRange = unit.visionRange; 
            IDamageable newTarget = unit.ScanForEnemies(visionRange);
            
            if (newTarget != null)
            {
                unit.stateMachine.ChangeState(new UnitState_Chase(newTarget));
            }
            else
            {
                unit.stateMachine.ChangeState(new UnitState_Idle());
            }
            return;
        }

        // Look at target
        Vector3 direction = (target.GetTransform().position - unit.transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            unit.transform.rotation = Quaternion.Slerp(unit.transform.rotation, lookRotation, Time.deltaTime * 5f);
        }

        // 📏 Check Distance to stay in state
        float range = unit.GetAttackRange(target);
        float currentDist = GetSurfaceDistance(unit, target);

        // Exit Condition: If we drifted significantly away, RE-CHASE
        // We add a small buffer (0.5f) to prevent jittering at the exact boundary
        if (currentDist > range + 0.5f)
        {
            unit.stateMachine.ChangeState(new UnitState_Chase(target));
            return;
        }

        // ⚔️ Perform Attack
        attackTimer += Time.deltaTime;
        float attackRate = (unit.data != null) ? unit.data.attackRate : 1.5f;

        if (attackTimer >= attackRate)
        {
            // Reset timer and Attack
            // We removed the strict inner check to prevent the "Frozen Unit" bug.
            // If the unit is close enough to stay in this state (checked above), it's close enough to attack.
            attackTimer = 0f;
            unit.TryAttack(target);
        }
    }

    private float GetSurfaceDistance(Unit unit, IDamageable target)
    {
        Collider targetCol = target.GetCollider();
        if (targetCol != null)
        {
            Vector3 closestPoint = targetCol.ClosestPoint(unit.transform.position);
            return Vector3.Distance(unit.transform.position, closestPoint);
        }
        
        // Fallback
        float d = Vector3.Distance(unit.transform.position, target.GetTransform().position);
        return Mathf.Max(0, d - target.GetRadius());
    }

    public void Exit(Unit unit)
    {
        // No distinct exit logic needed yet
    }
}

