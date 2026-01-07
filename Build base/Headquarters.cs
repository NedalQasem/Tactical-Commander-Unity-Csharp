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
        
        if (team == Unit.Team.Player)
        {
            Debug.Log("❌ GAME OVER - YOU LOST");
            // هنا نستدعي GameManager.LoseGame()
        }
        else
        {
            Debug.Log("🏆 VICTORY - ENEMY DESTROYED");
            // هنا نستدعي GameManager.WinGame()
        }

        base.Die();
    }
}
