using UnityEngine;

public class UnitState_Attack : IUnitState
{
    private Unit unit;
    private IDamageable target;
    private float attackTimer = 0f;
    private float attackRate = 1.0f; 

    public UnitState_Attack(IDamageable target)
    {
        this.target = target;
    }

    public void Enter(Unit unit)
    {
        this.unit = unit;
        if (unit.agent != null) unit.agent.isStopped = true; 
    }

    public void Update(Unit unit)
    {
        // 1. تحقق من وجود الهدف وحياته
        if (target == null || !target.IsAlive())
        {
            unit.target = null; // نسيان الهدف الميت
            unit.stateMachine.ChangeState(new UnitState_Idle());
            return;
        }

        // 2. الدوران نحو الهدف
        Vector3 direction = (target.GetTransform().position - unit.transform.position).normalized;
        direction.y = 0; 
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            unit.transform.rotation = Quaternion.Slerp(unit.transform.rotation, lookRotation, Time.deltaTime * 5f);
        }

        // 3. التحقق من المسافة (هل هرب؟)
        float dist = Vector3.Distance(unit.transform.position, target.GetTransform().position);
        float range = (unit.data != null) ? unit.data.attackRange : 2.0f;
        float targetRadius = target.GetRadius();

        if (dist > range + targetRadius + 0.5f) 
        {
            unit.stateMachine.ChangeState(new UnitState_Chase(target));
            return;
        }

        // 4. تنفيذ الهجوم
        attackTimer += Time.deltaTime;
        if (attackTimer >= attackRate)
        {
            PerformAttack();
            attackTimer = 0f;
        }
    }

    void PerformAttack()
    {
        int dmg = (unit.data != null) ? unit.data.attackDamage : 10;
        target.TakeDamage(dmg);
        // Debug.Log("HitTarget!");
    }

    public void Exit(Unit unit)
    {
        
    }
}
