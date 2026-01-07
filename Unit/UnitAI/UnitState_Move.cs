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
        // Check if we reached the destination
        if (unit.agent != null && !unit.agent.pathPending)
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
