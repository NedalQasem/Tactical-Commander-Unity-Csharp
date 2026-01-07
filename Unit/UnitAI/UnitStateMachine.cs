using UnityEngine;

public class UnitStateMachine
{
    private Unit unit;
    private IUnitState currentState;

    public UnitStateMachine(Unit unit)
    {
        this.unit = unit;
    }

    public void Initialize(IUnitState startingState)
    {
        ChangeState(startingState);
    }

    public void Update()
    {
        if (currentState != null)
        {
            currentState.Update(unit);
        }
    }

    public void ChangeState(IUnitState newState)
    {
        if (currentState != null)
        {
            currentState.Exit(unit);
        }

        currentState = newState;

        if (currentState != null)
        {
            currentState.Enter(unit);
        }
    }
}
