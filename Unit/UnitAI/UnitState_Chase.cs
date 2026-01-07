using UnityEngine;

public class UnitState_Chase : IUnitState
{
    private Unit unit;
    private IDamageable target; // ⬅️ تعديل النوع
    private float updateTimer = 0f;

    public UnitState_Chase(IDamageable target)
    {
        this.target = target;
    }

    public void Enter(Unit unit)
    {
        this.unit = unit;
        if (target != null && unit.agent != null)
        {
            unit.agent.isStopped = false;
            unit.agent.SetDestination(target.GetTransform().position);
        }
    }

    public void Update(Unit unit)
    {
        // 1. تحقق: هل العدو ما زال موجوداً؟
        if (target == null || !target.IsAlive())
        {
            unit.target = null;
            unit.stateMachine.ChangeState(new UnitState_Idle());
            return;
        }

        // 2. تحديث المسار
        updateTimer += Time.deltaTime;
        if (updateTimer > 0.2f)
        {
            unit.agent.SetDestination(target.GetTransform().position);
            updateTimer = 0f;
        }

        // 3. فحص المسافة (مع مراعاة نصف قطر الهدف للمباني الكبيرة)
        float dist = Vector3.Distance(unit.transform.position, target.GetTransform().position);
        float attackRange = (unit.data != null) ? unit.data.attackRange : 2.0f;
        
        // نضيف نصف قطر الهدف للمسافة (إذا كان مبنى كبيراً نقترب من حافته وليس مركزه)
        float targetRadius = target.GetRadius();

        if (dist <= attackRange + targetRadius)
        {
            unit.stateMachine.ChangeState(new UnitState_Attack(target));
        }
        else
        {
             unit.agent.isStopped = false;
        }
    }

    public void Exit(Unit unit)
    {
        if (unit.agent != null) unit.agent.isStopped = true;
    }
}
