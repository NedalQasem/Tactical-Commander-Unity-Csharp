using UnityEngine;

public class RangedUnit : Unit
{
    [Header("Ranged Settings")]
    public Transform firePoint; // Specific to shooting

    public override float GetAttackRange(IDamageable target)
    {
        // 🏹 Standard Vision/Attack Range
        return (data != null) ? data.attackRange : 10f;
    }

    public override void TryAttack(IDamageable target)
    {
        if (unitAnimation != null) unitAnimation.PlayAttack();

        // Spawn Projectile
        if (data != null && data.projectilePrefab != null)
        {
            Transform spawnPoint = (firePoint != null) ? firePoint : transform; // Fallback to self
            GameObject projObj = Instantiate(data.projectilePrefab, spawnPoint.position, spawnPoint.rotation);
            
            Projectile projectile = projObj.GetComponent<Projectile>();
            if (projectile != null)
            {
                int dmg = data.attackDamage;
                projectile.Setup(target, dmg, this.team);
            }
        }
        else
        {
            Debug.LogWarning($"⚠️ RangedUnit {name} has no Projectile Prefab assigned in Data!");
        }
    }

    protected override void Awake()
    {
        base.Awake(); // Call Unit.Awake to init Agent and Animation
        
        if (firePoint == null)
        {
            // Auto-find if not assigned
            Transform autoFirePoint = transform.Find("FirePoint");
            if (autoFirePoint != null) firePoint = autoFirePoint;
            // else FirePoint remains null and defaults to transform.position in TryAttack
        }
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        // Additional gizmos for FirePoint if needed
        if (firePoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(firePoint.position, 0.2f);
        }
    }
}

