using UnityEngine;

[RequireComponent(typeof(ResourceManager))]
[RequireComponent(typeof(EnemyAI_Builder))]
[RequireComponent(typeof(EnemyAI_Trainer))]
[RequireComponent(typeof(EnemyAI_Commander))]
public class EnemyAIController : MonoBehaviour
{
    private ResourceManager resourceManager;
    private EnemyAI_Builder builder;
    private EnemyAI_Trainer trainer;
    private EnemyAI_Commander commander;

    [Header("Global Settings")]
    public Transform enemyBaseCenter; 
    public Transform playerBaseTarget; 
    
    private float decisionTimer = 0f;
    private float incomeTimer = 0f;

    void Start()
    {
        resourceManager = GetComponent<ResourceManager>();
        resourceManager.team = Unit.Team.Enemy;

        builder = GetComponent<EnemyAI_Builder>();
        trainer = GetComponent<EnemyAI_Trainer>();
        commander = GetComponent<EnemyAI_Commander>();

        // Link Components
        builder.Initialize(this, resourceManager);
        trainer.Initialize(this, resourceManager, builder, commander);
        commander.Initialize(this);
    }

    void Update()
    {
        HandleIncome();
        
        // AI Tick (Decision Making)
        decisionTimer += Time.deltaTime;
        if (decisionTimer > 2.0f)
        {
            builder.Execute();
            trainer.Execute();
            decisionTimer = 0f;
        }

        // Combat Tick (Always Active)
        commander.Execute();
    }

    void HandleIncome()
    {
        incomeTimer += Time.deltaTime;
        if (incomeTimer >= 5.0f)
        {
            int income = 10;
            // Income logic can be moved to a separate Economy component if desired, 
            // but keeping it simple here or in Builder is fine.
            // For now, calculating based on Builder's current buildings.
            int mineCount = builder.GetBuildings().FindAll(b => b != null && b.name.Contains("Mine")).Count;
            if (mineCount > 0) income += mineCount * 5; 
            
            resourceManager.AddGold(income);
            incomeTimer = 0f;
        }
    }
}
