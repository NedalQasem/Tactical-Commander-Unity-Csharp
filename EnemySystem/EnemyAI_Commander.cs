using UnityEngine;
using System.Collections.Generic;

public class EnemyAI_Commander : MonoBehaviour
{
    private EnemyAIController controller;
    private List<Unit> attackingArmy = new List<Unit>();

    public void Initialize(EnemyAIController ctrl)
    {
        controller = ctrl;
    }

    public void LaunchWave(List<Unit> waveUnits)
    {
        Debug.Log($"🔥 Enemy Commander: Launching Wave! Received {waveUnits.Count} units.");
        
        // Create a COPY to avoid collection modification error
        List<Unit> waveCopy = new List<Unit>(waveUnits);
        
        int transferredCount = 0;
        foreach (var unit in waveCopy)
        {
            if (unit != null && unit.IsAlive())
            {
                attackingArmy.Add(unit);
                
                // Allow Aggression
                unit.isPassive = false;

                // Give each unit a slightly different target position to spread the attack
                if (controller.playerBaseTarget != null)
                {
                    Vector3 targetOffset = new Vector3(
                        Random.Range(-5f, 5f),
                        0,
                        Random.Range(-5f, 5f)
                    );
                    Vector3 attackPosition = controller.playerBaseTarget.position + targetOffset;
                    unit.MoveTo(attackPosition);
                }
                transferredCount++;
            }
            else
            {
                Debug.LogWarning($"⚠️ COMMANDER: Unit {unit?.name ?? "NULL"} is dead or null!");
            }
        }
        Debug.Log($"✅ COMMANDER: Transferred {transferredCount} units. Total Attacking Army: {attackingArmy.Count}");
    }

    public void Execute()
    {
        attackingArmy.RemoveAll(u => u == null || !u.IsAlive());

        // DISABLED: Continuous re-targeting causes one-by-one movement
        // Units will use natural auto-aggro from UnitState_Idle instead
        
        /*
        if (controller.playerBaseTarget == null) return;

        foreach (var unit in attackingArmy)
        {
            if (unit != null && unit.IsAlive())
            {
                // Maintain Pressure
                if (unit.target == null)
                {
                     unit.MoveTo(controller.playerBaseTarget.position);
                }
            }
        }
        */
    }
}
