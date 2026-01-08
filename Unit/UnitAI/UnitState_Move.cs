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
        if (unit.IsAgentReady)
        {
            unit.agent.isStopped = false; // 🟢 Ensure agent can move!
            unit.agent.SetDestination(targetPosition);
        }
    }

    public void Update(Unit unit)
    {
        // 🛡️ Conditional Logic:
        // Player: Standard Move (Disengage/Ignore enemies)
        // Enemy: Attack Move (Aggressive - Attack if they see someone)
        if (unit.team == Unit.Team.Enemy)
        {
            if (unit.FindClosestEnemy())
            {
                if (unit.target != null)
                {
                    unit.stateMachine.ChangeState(new UnitState_Chase(unit.target));
                    return;
                }
            }
        }

        // Check if we reached the destination
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

