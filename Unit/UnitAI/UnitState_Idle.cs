using UnityEngine;

public class UnitState_Idle : IUnitState
{
    public void Enter(Unit unit)
    {
        if (unit.IsAgentReady) 
        {
            unit.agent.ResetPath();
        }
    }

    private float detectionTimer = 0f;

    public void Update(Unit unit)
    {
        detectionTimer += Time.deltaTime;
        if (detectionTimer > 0.5f) 
        {
            if (!unit.isPassive && unit.FindClosestEnemy())
            {
                unit.stateMachine.ChangeState(new UnitState_Chase(unit.target));
            }
            detectionTimer = 0f;
        }
    }

    public void Exit(Unit unit)
    {
        
    }
}
