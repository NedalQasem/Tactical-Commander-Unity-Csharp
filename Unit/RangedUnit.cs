using UnityEngine;

public class RangedUnit : Unit
{
    [Header("Ranged Settings")]
    public Transform firePoint; // Specific to shooting

    public override float GetAttackRange(IDamageable target)
    {
        return (data != null) ? data.attackRange : 10f;
    }

    public override void TryAttack(IDamageable target)
    {
        if (unitAnimation != null) unitAnimation.PlayAttack();

        if (data != null && data.projectilePrefab != null)
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFXAt(SoundType.UnitAttack, transform.position);

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
            Debug.LogWarning($" RangedUnit {name} has no Projectile Prefab assigned in Data!");
        }
    }

    protected override void Awake()
    {
        base.Awake();
        
        if (firePoint == null)
        {
            Transform autoFirePoint = transform.Find("FirePoint");
            if (autoFirePoint != null) firePoint = autoFirePoint;
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

