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

    void Update()
    {
        // 1. زيادة الذهب بناءً على المناجم
        // الدخل الأساسي (قليل جداً) + دخل المناجم
        float baseIncome = 1.0f; // 1 ذهب في الثانية
        float minesIncome = CountBuildings("Mine") * 5.0f; // كل منجم يعطي 5 ذهب/ثانية
        
        float totalRate = baseIncome + minesIncome;
        
        // نستخدم متغير كسري (float) لتجميع الذهب بمرور الوقت
        // نحتاج لمتغير خاص لتخزين الكسور (سأضيفه الآن كحقل خاص)
        accumulator += totalRate * Time.deltaTime;
        if (accumulator >= 1.0f)
        {
            int gain = (int)accumulator;
            currentGold += gain;
            accumulator -= gain;
        }

        decisionTimer += Time.deltaTime;
        if (decisionTimer > 2.0f)
        {
            MakeDecision();
            decisionTimer = 0f;
        }
    }

    private float accumulator = 0f; // لتجميع كسور الذهب

    void MakeDecision()
    {
        // 🔥 الأولوية 0: الهجوم! (تفقد هذا أولاً)
        if (myArmy.Count >= attackThreshold)
        {
            LaunchAttack();
            return;
        }

        // الأولوية 1: بناء منجم إذا لم يوجد
        if (CountBuildings("Mine") < 2 && currentGold >= 50)
        {
            TryBuildBuilding(minePrefab, 50, "Mine");
            return;
        }

        // الأولوية 2: بناء ثكنة
        if (CountBuildings("Barracks") < 1 && currentGold >= 50)
        {
            TryBuildBuilding(barracksPrefab, 50, "Barracks");
            return;
        }

        // الأولوية 3: تدريب جنود
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
            
            // ⚠️ تعيين الفريق وتفعيل المبنى
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

        // ابحث عن ثكنة
        foreach(var b in myBuildings)
        {
            if (b.name.Contains("Barracks")) 
            {
                currentGold -= 10;
                
                // اختيار جندي عشوائي من القائمة 🎲
                GameObject randomUnitPrefab = unitPrefabs[Random.Range(0, unitPrefabs.Count)];

                // إنشاء الجندي بجانب الثكنة
                Vector3 spawnPos = b.transform.position + Vector3.forward * 2;
                GameObject u = Instantiate(randomUnitPrefab, spawnPos, Quaternion.identity);
                
                Unit unitScript = u.GetComponent<Unit>();
                if (unitScript != null)
                {
                    unitScript.team = Unit.Team.Enemy; // 🔴 تعيين الفريق عدو
                    myArmy.Add(unitScript);
                }
                Debug.Log($"😈 Enemy Trained Unit: {u.name}");
                break; 
            }
        }
    }

    void LaunchAttack()
    {
        Debug.Log("⚔️🔥 ENEMY ATTACK LAUNCHED! 🔥⚔️");
        foreach (var unit in myArmy)
        {
            if (unit != null && unit.IsAlive())
            {
                unit.MoveTo(playerBaseTarget.position);
                // اجعلهم بوضع هجومي (Attack Move)
                // unit.stateMachine.ChangeState(new UnitState_AttackMove(...));
            }
        }
        myArmy.Clear(); // انسَهم، فليذهبوا للموت! (أو انقلهم لقائمة "AttackingSquad")
    }

    // 👷‍♂️ البحث عن أرض فارغة وصالحة للبناء
    Vector3 FindBuildPosition()
    {
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
        
        Debug.LogWarning("⚠️ EnemyAI: Could not find valid build position after 30 tries.");
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
