using UnityEngine;

public class UnitState_Idle : IUnitState
{
    public void Enter(Unit unit)
    {
        // تحقق: هل الـ Agent موجود وهل هو فعلاً على الأرض؟
        if (unit.IsAgentReady) 
        {
            unit.agent.ResetPath();
        }
    }

    private float detectionTimer = 0f;

    public void Update(Unit unit)
    {
        detectionTimer += Time.deltaTime;
        if (detectionTimer > 0.5f) // بحث كل نصف ثانية
        {
            if (unit.FindClosestEnemy())
            {
                unit.stateMachine.ChangeState(new UnitState_Chase(unit.target)); // ⬅️ تصحيح الاسم
            }
            detectionTimer = 0f;
        }
    }

    public void Exit(Unit unit)
    {
        
    }
}
