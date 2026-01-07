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
        if (target == null || !target.IsAlive())
        {
            unit.stateMachine.ChangeState(new UnitState_Idle());
            return;
        }

        // Look at target
        Vector3 direction = (target.GetTransform().position - unit.transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            unit.transform.rotation = Quaternion.Slerp(unit.transform.rotation, lookRotation, Time.deltaTime * 5f);
        }

        bool isMelee = (unit.data != null && unit.data.attackType == UnitData.AttackType.Melee);
        
        if (isMelee)
        {
            HandleMeleeCombat(unit);
        }
        else
        {
            HandleRangedCombat(unit);
        }
    }

    // ⚔️ Strict Melee Logic
    private void HandleMeleeCombat(Unit unit)
    {
        float surfaceDist = GetSurfaceDistance(unit, target);
        
        // Exit Condition: If we drifted away more than a tiny bit, RE-CHASE
        if (surfaceDist > 0.5f) 
        {
            unit.stateMachine.ChangeState(new UnitState_Chase(target));
            return;
        }

        // Attack Logic
        attackTimer += Time.deltaTime;
        float attackRate = (unit.data != null) ? unit.data.attackRate : 1.5f;

        if (attackTimer >= attackRate)
        {
            // 🔒 ZERO DISTANCE ENFORCEMENT
            // Only deal damage if we are virtually touching
            if (surfaceDist <= 0.3f) 
            {
                PerformMeleeAttack(unit);
                attackTimer = 0f;
            }
        }
    }

    // 🏹 Standard Ranged Logic
    private void HandleRangedCombat(Unit unit)
    {
        float surfaceDist = GetSurfaceDistance(unit, target);
        float range = (unit.data != null) ? unit.data.attackRange : 5.0f;

        // Exit Condition
        if (surfaceDist > range + 0.5f)
        {
            unit.stateMachine.ChangeState(new UnitState_Chase(target));
            return;
        }

        // Fire Logic
        attackTimer += Time.deltaTime;
        float attackRate = (unit.data != null) ? unit.data.attackRate : 1.5f;

        if (attackTimer >= attackRate)
        {
            PerformRangedAttack(unit);
            attackTimer = 0f;
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

    private void PerformMeleeAttack(Unit unit)
    {
        if (unit.unitAnimation != null) unit.unitAnimation.PlayAttack();
        int dmg = (unit.data != null) ? unit.data.attackDamage : 10;
        target.TakeDamage(dmg);
    }

    private void PerformRangedAttack(Unit unit)
    {
        if (unit.unitAnimation != null) unit.unitAnimation.PlayAttack();
        int dmg = (unit.data != null) ? unit.data.attackDamage : 10;

        if (unit.data != null && unit.data.projectilePrefab != null)
        {
            Transform spawnPoint = (unit.firePoint != null) ? unit.firePoint : unit.transform;
            GameObject projObj = Object.Instantiate(unit.data.projectilePrefab, spawnPoint.position, spawnPoint.rotation);
            Projectile projectile = projObj.GetComponent<Projectile>();
            if (projectile != null) projectile.Setup(target, dmg, unit.team);
        }
    }

    public void Exit(Unit unit)
    {
        // No distinct exit logic needed yet
    }
}
