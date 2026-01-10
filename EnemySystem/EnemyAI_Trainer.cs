using UnityEngine;
using System.Collections.Generic;

public class EnemyAI_Trainer : MonoBehaviour
{
    [Header("Settings")]
    public int attackThreshold = 5;
    public List<GameObject> unitPrefabs;

    private EnemyAIController controller;
    private ResourceManager resourceManager;
    private EnemyAI_Builder builder;
    private EnemyAI_Commander commander;

    public List<Unit> reserveArmy = new List<Unit>();

    public void Initialize(EnemyAIController ctrl, ResourceManager rm, EnemyAI_Builder b, EnemyAI_Commander c)
    {
        controller = ctrl;
        resourceManager = rm;
        builder = b;
        commander = c;
    }

    public void Execute()
    {
        // Cleanup
        reserveArmy.RemoveAll(u => u == null || !u.IsAlive());

        // Check for Wave Launch
        if (reserveArmy.Count >= attackThreshold)
        {
            Debug.Log($"🔥 TRAINER: Launching Wave! Reserve: {reserveArmy.Count} / Threshold: {attackThreshold}");
            commander.LaunchWave(reserveArmy);
            reserveArmy.Clear();
        }
        else
        {
            Debug.Log($"📦 TRAINER: Reserve Army: {reserveArmy.Count} / {attackThreshold}");
        }

        // Production Logic
        // Only train if we have a Barracks and some gold
        if (resourceManager.GetCurrentGold() >= 10) // Basic check
        {
            TryTrainUnit();
        }
    }

    void TryTrainUnit()
    {
        if (unitPrefabs == null || unitPrefabs.Count == 0) return;

        // Find Barracks from Builder
        List<GameObject> buildings = builder.GetBuildings();
        GameObject barracks = null;
        foreach(var b in buildings)
        {
            if (b != null && b.name.Contains("Barracks")) 
            {
                barracks = b;
                break;
            }
        }

        if (barracks == null) return;

        List<GameObject> affordableUnits = new List<GameObject>();
        foreach(var prefab in unitPrefabs)
        {
            if (prefab == null) continue;
            Unit unitScript = prefab.GetComponent<Unit>();
            if (unitScript != null && unitScript.data != null)
            {
                if (resourceManager.GetCurrentGold() >= unitScript.data.goldCost)
                    affordableUnits.Add(prefab);
            }
        }

        if (affordableUnits.Count > 0)
        {
            GameObject chosenPrefab = affordableUnits[Random.Range(0, affordableUnits.Count)];
            Unit unitDataScript = chosenPrefab.GetComponent<Unit>();
            int cost = unitDataScript.data.goldCost;

            if (resourceManager.TrySpendGold(cost))
            {
                // Spawn with random offset to prevent stacking
                Vector3 randomOffset = new Vector3(
                    Random.Range(-3f, 3f),
                    0,
                    Random.Range(-3f, 3f)
                );
                Vector3 spawnPos = barracks.transform.position + Vector3.forward * 2 + randomOffset;
                GameObject u = Instantiate(chosenPrefab, spawnPos, Quaternion.identity);
                
                Unit newUnitScript = u.GetComponent<Unit>();
                if (newUnitScript != null)
                {
                    newUnitScript.team = Unit.Team.Enemy;
                    newUnitScript.isPassive = true; // Set to Passive Logic
                    
                    // Configure NavMeshAgent for group movement
                    if (newUnitScript.agent != null)
                    {
                        newUnitScript.agent.radius = 0.3f;           // Smaller collision radius
                        newUnitScript.agent.stoppingDistance = 0.5f; // Closer stopping distance
                        newUnitScript.agent.avoidancePriority = 40;  // Lower priority = less waiting
                    }
                    
                    reserveArmy.Add(newUnitScript);
                }
                Debug.Log($"😈 Enemy Trainer: Trained {u.name}");
            }
        }
    }
}
