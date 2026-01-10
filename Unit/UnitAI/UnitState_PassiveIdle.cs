using UnityEngine;

public class UnitState_PassiveIdle : IUnitState
{
    public void Enter(Unit unit)
    {
        if (unit.IsAgentReady) 
        {
            unit.agent.ResetPath();
        }
    }

    public void Update(Unit unit)
    {
        // Passive State: Do NOT scan for enemies automatically.
        // The unit waits until it receives a command (e.g., MoveTo) which changes the state.
    }

    public void Exit(Unit unit)
    {
        
    }
}
