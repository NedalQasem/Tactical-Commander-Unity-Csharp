using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Unit.Team shooterTeam; // 🛡️ فريق المطلق
    private IDamageable target;
    private int damage;
    private float speed = 15f; // سرعة القذيفة

    public void Setup(IDamageable target, int damage, Unit.Team team)
    {
        this.target = target;
        this.damage = damage;
        this.shooterTeam = team;
        // Debug.Log($"🚀 Projectile Fired! Target: {target.GetTransform().name}");
        
        // تدمير ذاتي بعد 5 ثواني في حال ضاعت القذيفة
        Destroy(gameObject, 5f);
    }

    private void Start()
    {
        // if (target == null) Debug.LogWarning("⚠️ Projectile created but target is NULL in Start! (Wait for Setup)");
    }

    private void Update()
    {
        if (target == null || !target.IsAlive())
        {
            Destroy(gameObject); // تدمير القذيفة إذا مات الهدف قبل الوصول
            return;
        }

        // التحرك نحو الهدف
        Vector3 dir = (target.GetTransform().position - transform.position).normalized;
        // نرفع الهدف قليلاً (Y+1) لنضرب صدر الهدف وليس قدميه
        Vector3 targetPos = target.GetTransform().position + Vector3.up * 1.0f; 
        
        // إعادة حساب الاتجاه ليشير للمنتصف
        dir = (targetPos - transform.position).normalized;

        transform.position += dir * speed * Time.deltaTime;
        transform.LookAt(targetPos); // اجعل السهم ينظر للهدف

        // الكشف عن الاصطدام
        float distanceThisFrame = speed * Time.deltaTime;
        if (Vector3.Distance(transform.position, targetPos) <= distanceThisFrame)
        {
            HitTarget();
        }
    }

    void HitTarget()
    {
        if (target != null && target.IsAlive())
        {
            // 🛡️ الحماية من النيران الصديقة
            if (target.GetTeam() != shooterTeam)
            {
                target.TakeDamage(damage);
            }
        }
        
        // هنا ممكن نشغل مؤثرات انفجار (Particle Effect) لاحقاً
        Destroy(gameObject);
    }
}
