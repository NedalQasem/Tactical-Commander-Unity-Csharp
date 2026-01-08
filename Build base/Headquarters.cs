using UnityEngine;

public class Headquarters : BuildingBase
{
    [Header("HQ Settings")]
    public float resourceGenerationRate = 5f; // يمكن أن يولد موارد ببطء لدعم اللاعب

    protected override void Awake()
    {
        base.Awake();
        maxHealth = 1000; // صحة عالية جداً
        currentHealth = maxHealth;
    }

    protected override void Die()
    {
        Debug.Log($"🚨 كارثة! تم تدمير المقر الرئيسي للفريق: {team}");
        
        // 🔍 Robust GameManager Lookup
        GameManager gm = GameManager.Instance;
        if (gm == null)
        {
            gm = FindFirstObjectByType<GameManager>();
            if (gm != null) Debug.Log("⚠️ Headquarters: Found GameManager via fallback search (Instance was null).");
        }

        if (team == Unit.Team.Player)
        {
            Debug.Log("❌ GAME OVER - YOU LOST");
            if (gm != null) gm.EndGame(false);
            else Debug.LogError("❌ Headquarters: Cannot trigger Defeat - GameManager Instance is MISSING!");
        }
        else
        {
            Debug.Log("🏆 VICTORY - ENEMY DESTROYED");
            if (gm != null) gm.EndGame(true);
            else Debug.LogError("❌ Headquarters: Cannot trigger Victory - GameManager Instance is MISSING!");
        }

        base.Die();
    }
}
