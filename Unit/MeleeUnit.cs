using UnityEngine;

public class MeleeUnit : Unit
{
    public float attackContactBuffer = 0.2f; // Extra distance to ensure contact

    public override float GetAttackRange(IDamageable target)
    {
        if (target == null) return 1.0f;

        float myRadius = GetRadius();
        float targetRadius = target.GetRadius();

        return myRadius + targetRadius + attackContactBuffer;
    }

    public override void TryAttack(IDamageable target)
    {
        if (unitAnimation != null) unitAnimation.PlayAttack();

        if (target.GetTeam() == this.team) return;

        // 🔊 Attack Sound
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFXAt(SoundType.UnitAttack, transform.position);

        int dmg = (data != null) ? data.attackDamage : 10;
        target.TakeDamage(dmg);
    }
}
