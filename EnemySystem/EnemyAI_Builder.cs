using UnityEngine;
using System.Collections.Generic;

public class EnemyAI_Builder : MonoBehaviour
{
    [Header("Settings")]
    public float buildRadius = 20f;
    public LayerMask obstacleMask;

    [Header("Prefabs")]
    public GameObject minePrefab;
    public GameObject barracksPrefab;

    private EnemyAIController controller;
    private ResourceManager resourceManager;
    private List<GameObject> builtBuildings = new List<GameObject>();

    public void Initialize(EnemyAIController ctrl, ResourceManager rm)
    {
        controller = ctrl;
        resourceManager = rm;
    }

    public void Execute()
    {
        int currentGold = resourceManager.GetCurrentGold();

        // Priority 1: Mines (Economy)
        if (CountBuildings("Mine") < 2 && currentGold >= GetBuildingCost("Mine"))
        {
            if (TryBuildBuilding(minePrefab, GetBuildingCost("Mine"), "Mine")) return;
        }

        // Priority 2: Barracks (Production)
        if (CountBuildings("Barracks") < 1 && currentGold >= GetBuildingCost("Barracks"))
        {
            if (TryBuildBuilding(barracksPrefab, GetBuildingCost("Barracks"), "Barracks")) return;
        }
    }

    public List<GameObject> GetBuildings()
    {
        return builtBuildings;
    }

    int CountBuildings(string namePart)
    {
        int count = 0;
        foreach (var b in builtBuildings)
            if (b != null && b.name.Contains(namePart)) count++;
        return count;
    }

    int GetBuildingCost(string type)
    {
        if (type == "Mine") return 50;
        if (type == "Barracks") return 50;
        return 50;
    }

    bool TryBuildBuilding(GameObject prefab, int cost, string tag)
    {
        Vector3 buildPos = FindBuildPosition();
        if (buildPos != Vector3.zero)
        {
            if (resourceManager.TrySpendGold(cost))
            {
                GameObject b = Instantiate(prefab, buildPos, Quaternion.identity);
                BuildingBase buildingScript = b.GetComponent<BuildingBase>();
                if (buildingScript != null)
                {
                    buildingScript.team = Unit.Team.Enemy;
                    buildingScript.isPlaced = true;
                }
                builtBuildings.Add(b);
                Debug.Log($"😈 Enemy Builder: Built {tag}");
                return true;
            }
        }
        return false;
    }

    Vector3 FindBuildPosition()
    {
        if (controller.enemyBaseCenter == null) return Vector3.zero;

        float searchRadius = Mathf.Max(buildRadius, 10f);

        for (int i = 0; i < 30; i++) 
        {
            Vector2 randomPoint = Random.insideUnitCircle * searchRadius;
            Vector3 potentialPos = controller.enemyBaseCenter.position + new Vector3(randomPoint.x, 0, randomPoint.y);

            UnityEngine.AI.NavMeshHit hit;
            if (UnityEngine.AI.NavMesh.SamplePosition(potentialPos, out hit, 2.0f, UnityEngine.AI.NavMesh.AllAreas))
            {
                Vector3 finalPos = hit.position;
                
                // 2. Robust Obstacle Check (Radius increased to 6.0f)
                Collider[] colliders = Physics.OverlapSphere(finalPos, 6.0f);
                bool isBlocked = false;
                foreach (var col in colliders)
                {
                    // Ignore Ground/Terrain
                    if (col.gameObject.layer == LayerMask.NameToLayer("Ground") || col.CompareTag("Ground")) continue;
                    if (col.isTrigger) continue;

                    // Block on Buildings or Units
                    if (col.GetComponent<BuildingBase>() != null || col.GetComponent<Unit>() != null) 
                    {
                        isBlocked = true;
                        break;
                    }
                }
                
                if (!isBlocked) 
                {
                    return finalPos;
                }
            }
        }
        Debug.LogWarning("⚠️ Enemy Builder: No valid spot found.");
        return Vector3.zero; 
    }
}
