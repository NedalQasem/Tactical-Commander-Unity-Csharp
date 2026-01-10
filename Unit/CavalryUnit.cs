using UnityEngine;
/// <summary>
/// Cavalry Unit - وحدة فرسان سريعة بمدى هجوم متوسط
/// </summary>
public class CavalryUnit : Unit
{
    [Header("Cavalry Settings")]
    [SerializeField] private float chargeBonus = 1.5f; // ضرر إضافي عند الشحن
    [SerializeField] private float chargeSpeedThreshold = 4f; // السرعة المطلوبة للشحن
    private bool isCharging = false;
    public override float GetAttackRange(IDamageable target)
    {
        if (target == null) return 2.5f;
        // مدى الهجوم = نصف قطري + نصف قطر الهدف + مسافة إضافية
        float myRadius = GetRadius();
        float targetRadius = target.GetRadius();
        
        return myRadius + targetRadius + 0.5f; // مدى أكبر من Melee العادي
    }
    public override void TryAttack(IDamageable target)
    {
        if (unitAnimation != null) unitAnimation.PlayAttack();
        // 🛡️ منع الـ Friendly Fire
        if (target.GetTeam() == this.team) return;
        // 🔊 صوت الهجوم
        if (AudioManager.Instance != null) 
            AudioManager.Instance.PlaySFXAt(SoundType.UnitAttack, transform.position);
        // 💥 حساب الضرر (مع بونص الشحن إذا كان يجري بسرعة)
        int baseDamage = (data != null) ? data.attackDamage : 30;
        float finalDamage = baseDamage;
        if (isCharging)
        {
            finalDamage = baseDamage * chargeBonus;
            Debug.Log($"⚡ Cavalry Charge Bonus! Damage: {finalDamage}");
        }
        target.TakeDamage(Mathf.RoundToInt(finalDamage));
    }
    protected override void Update()
    {
        base.Update();
        // ⚡ تحديد حالة الشحن بناءً على السرعة
        if (IsAgentReady)
        {
            isCharging = agent.velocity.magnitude >= chargeSpeedThreshold;
        }
    }
}