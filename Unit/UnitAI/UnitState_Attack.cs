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
        if ((target as UnityEngine.Object) == null || !target.IsAlive())
        {
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

        Vector3 direction = (target.GetTransform().position - unit.transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            unit.transform.rotation = Quaternion.Slerp(unit.transform.rotation, lookRotation, Time.deltaTime * 5f);
        }

        float range = unit.GetAttackRange(target);
        float currentDist = GetSurfaceDistance(unit, target);

        if (currentDist > range + 0.5f)
        {
            unit.stateMachine.ChangeState(new UnitState_Chase(target));
            return;
        }

        attackTimer += Time.deltaTime;
        float attackRate = (unit.data != null) ? unit.data.attackRate : 1.5f;

        if (attackTimer >= attackRate)
        {
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
        
        
        float d = Vector3.Distance(unit.transform.position, target.GetTransform().position);
        return Mathf.Max(0, d - target.GetRadius());
    }

    public void Exit(Unit unit)
    {
    }
}

