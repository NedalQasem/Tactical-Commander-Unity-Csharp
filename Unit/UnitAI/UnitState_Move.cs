using UnityEngine;

public class UnitState_Move : IUnitState
{
    private Vector3 targetPosition;

    public UnitState_Move(Vector3 targetPosition)
    {
        this.targetPosition = targetPosition;
    }

    public void Enter(Unit unit)
    {
        if (unit.agent != null) unit.agent.SetDestination(targetPosition);
    }

    public void Update(Unit unit)
    {
        // 🛡️ Attack Move Check: أثناء الحركة، افحص إذا ظهر عدو
        if (unit.FindClosestEnemy())
        {
            unit.stateMachine.ChangeState(new UnitState_Chase(unit));
            return;
        }

        // Check if we reached the destination
        // 🛡️ الحماية الكاملة: لا تسأل الـ Agent إلا إذا كان جاهزاً وعلى الأرض
        if (unit.IsAgentReady && !unit.agent.pathPending)
        {
            if (unit.agent.remainingDistance <= unit.agent.stoppingDistance)
            {
                // Reached destination -> Switch back to Idle
                unit.stateMachine.ChangeState(new UnitState_Idle());
            }
        }
    }

    public void Exit(Unit unit)
    {
        unit.StopMoving();
    }
}
