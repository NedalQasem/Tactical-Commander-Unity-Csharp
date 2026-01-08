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
            unit.agent.isStopped = false; 
            unit.agent.SetDestination(targetPosition);
        }
    }

    public void Update(Unit unit)
    {
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

        if (unit.IsAgentReady && !unit.agent.pathPending)
        {
            if (unit.agent.remainingDistance <= unit.agent.stoppingDistance)
            {
                unit.stateMachine.ChangeState(new UnitState_Idle());
            }
        }
    }

    public void Exit(Unit unit)
    {
        unit.StopMoving();
    }
}

