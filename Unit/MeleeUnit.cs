using UnityEngine;

public class MeleeUnit : Unit
{
    public float attackContactBuffer = 0.2f; // Extra distance to ensure contact

    public override float GetAttackRange(IDamageable target)
    {
        // 📏 Strict Physical Math for Melee Range
        // Must be close enough to basically touch
        if (target == null) return 1.0f;

        float myRadius = GetRadius();
        float targetRadius = target.GetRadius();

        return myRadius + targetRadius + attackContactBuffer;
    }

    public override void TryAttack(IDamageable target)
    {
        if (unitAnimation != null) unitAnimation.PlayAttack();

        // 🛡️ CRITICAL: Prevent Friendly Fire
        if (target.GetTeam() == this.team) return;

        // Apply Damage Directly (could be synced with Animation Event in future)
        int dmg = (data != null) ? data.attackDamage : 10;
        target.TakeDamage(dmg);
    }
}
