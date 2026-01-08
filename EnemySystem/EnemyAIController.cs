using UnityEngine;
using System.Collections.Generic;

public class EnemyAIController : MonoBehaviour
{
    [Header("Resources")]
    public int currentGold = 100;
    public float goldIncomeRate = 10f; // دخل تلقائي بسيط للبداية (أو يعتمد على المناجم)

    [Header("Prefabs & References")]
    public GameObject minePrefab;
    public GameObject barracksPrefab;
    public List<GameObject> unitPrefabs; // ⬅️ الآن أصبحت قائمة
    public Transform enemyBaseCenter; 
    public Transform playerBaseTarget; 
    
    [Header("AI Settings")]
    public float buildRadius = 20f; 
    public int attackThreshold = 5; 
    public LayerMask obstacleMask; // ⬅️ الإضافة هنا
    
    // Internal State
    private List<GameObject> myBuildings = new List<GameObject>();
    private List<Unit> myArmy = new List<Unit>();
    private float decisionTimer = 0f;

    private float incomeTimer = 0f;

    void Start()
    {
        currentGold = 100;
        
        goldIncomeRate = 0f; 
    }

    void Update()
    {
        //  Income Logic: 10 Gold every 5 Seconds
        incomeTimer += Time.deltaTime;
        if (incomeTimer >= 5.0f)
        {
            currentGold += 10;
            
            int mineCount = CountBuildings("Mine");
            if (mineCount > 0)
            {
                currentGold += mineCount * 5; 
            }
            
            incomeTimer = 0f;
        }

        decisionTimer += Time.deltaTime;
        if (decisionTimer > 2.0f)
        {
            MakeDecision();
            decisionTimer = 0f;
        }
    }

    private float accumulator = 0f;

    void MakeDecision()
    {
        if (myArmy.Count >= attackThreshold)
        {
            LaunchAttack();
            return;
        }
        if (CountBuildings("Mine") < 2 && currentGold >= 50)
        {
            TryBuildBuilding(minePrefab, 50, "Mine");
            return;
        }
        if (CountBuildings("Barracks") < 1 && currentGold >= 50)
        {
            TryBuildBuilding(barracksPrefab, 50, "Barracks");
            return;
        }
        if (CountBuildings("Barracks") > 0 && currentGold >= 10)
        {
            TrainUnit();
            return;
        }
    }

    void TryBuildBuilding(GameObject prefab, int cost, string tag)
    {
        Vector3 buildPos = FindBuildPosition();
        if (buildPos != Vector3.zero)
        {
            currentGold -= cost;
            GameObject b = Instantiate(prefab, buildPos, Quaternion.identity);
            
            BuildingBase buildingScript = b.GetComponent<BuildingBase>();
            if (buildingScript != null)
            {
                buildingScript.team = Unit.Team.Enemy;
                buildingScript.isPlaced = true;
            }
            
            myBuildings.Add(b);
            Debug.Log($"😈 Enemy Built: {tag} at {buildPos}");
        }
    }

    void TrainUnit()
    {
        if (unitPrefabs == null || unitPrefabs.Count == 0) return;

        // 1. Find a Barracks
        GameObject barracks = null;
        foreach(var b in myBuildings)
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
                if (currentGold >= unitScript.data.goldCost)
                {
                    affordableUnits.Add(prefab);
                }
            }
        }

        if (affordableUnits.Count > 0)
        {
            GameObject chosenPrefab = affordableUnits[Random.Range(0, affordableUnits.Count)];
            Unit unitDataScript = chosenPrefab.GetComponent<Unit>();
            int cost = unitDataScript.data.goldCost;

            currentGold -= cost;

            // Spawn
            Vector3 spawnPos = barracks.transform.position + Vector3.forward * 2;
            GameObject u = Instantiate(chosenPrefab, spawnPos, Quaternion.identity);
            
            Unit newUnitScript = u.GetComponent<Unit>();
            if (newUnitScript != null)
            {
                newUnitScript.team = Unit.Team.Enemy; // Assign Team Enemy
                myArmy.Add(newUnitScript);
            }
            Debug.Log($" Enemy Paid {cost} Gold to Train: {u.name}");
        }
    }

    void LaunchAttack()
    {
        Debug.Log(" ENEMY ATTACK LAUNCHED! ");
        foreach (var unit in myArmy)
        {
            if (unit != null && unit.IsAlive())
            {
                unit.MoveTo(playerBaseTarget.position);
                //(Attack Move)
            }
        }
        myArmy.Clear(); //("AttackingSquad")
    }

    //  البحث عن أرض فارغة وصالحة للبناء
    Vector3 FindBuildPosition()
    {
        //  Safety Check: If Base is destroyed, we can't calculate position relative to it
        if (enemyBaseCenter == null) return Vector3.zero;

        // Safety: If buildRadius is too small, default it
        float searchRadius = Mathf.Max(buildRadius, 10f);

        for (int i = 0; i < 30; i++) // Increased trials to 30
        {
            Vector2 randomPoint = Random.insideUnitCircle * searchRadius;
            Vector3 potentialPos = enemyBaseCenter.position + new Vector3(randomPoint.x, 0, randomPoint.y);

            // 1. فحص هل المكان على الـ NavMesh (أرض صالحة للمشي)
            UnityEngine.AI.NavMeshHit hit;
            if (UnityEngine.AI.NavMesh.SamplePosition(potentialPos, out hit, 2.0f, UnityEngine.AI.NavMesh.AllAreas))
            {
                Vector3 finalPos = hit.position;

                // 2. الفحص باستخدام الـ LayerMask المخصص
                // نرفع الكرة قليلاً (1 متر) لكي لا تلامس الأرض، ونستخدم الماسك
                bool hitObstacle = Physics.CheckSphere(finalPos + Vector3.up * 1.0f, 2.0f, obstacleMask);
                
                if (!hitObstacle) 
                {
                    return finalPos;
                }
            }
        }
        
        Debug.LogWarning(" EnemyAI: Could not find valid build position after 30 tries.");
        return Vector3.zero; // لم أجد مكاناً مناسباً
    }

    int CountBuildings(string namePart)
    {
        int count = 0;
        foreach (var b in myBuildings)
            if (b != null && b.name.Contains(namePart)) count++;
        return count;
    }
}
