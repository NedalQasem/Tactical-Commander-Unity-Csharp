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
        
        Destroy(gameObject, 5f);
    }

    private void Start()
    {
        // if (target == null) Debug.LogWarning(" Projectile created but target is NULL in Start! (Wait for Setup)");
    }

    private void Update()
    {
        if (target == null || !target.IsAlive())
        {
            Destroy(gameObject); 
            return;
        }

        Vector3 dir = (target.GetTransform().position - transform.position).normalized;
        Vector3 targetPos = target.GetTransform().position + Vector3.up * 1.0f; 
        
        dir = (targetPos - transform.position).normalized;

        transform.position += dir * speed * Time.deltaTime;
        transform.LookAt(targetPos);

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
            if (target.GetTeam() != shooterTeam)
            {
                target.TakeDamage(damage);
            }
        }
        Destroy(gameObject);
    }
}
