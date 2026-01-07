using UnityEngine;
using UnityEngine.AI;

public class UnitAnimation : MonoBehaviour
{
    private Animator animator;
    private NavMeshAgent agent;

    [Header("Animation Parameter Names")]
    [SerializeField] private string moveParameter = "IsMoved";
    [SerializeField] private string attackParameter = "Attack";

    private void Awake()
    {
        // البحث عن المكونات بذكاء (سواء على نفس الكائن أو الأبناء)
        animator = GetComponentInChildren<Animator>();
        if (animator == null) animator = GetComponent<Animator>();

        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (animator == null || agent == null) return;

        // 🧠 المنطق التلقائي: إذا كان يتحرك، شغل الركض
        // نستخدم sqrMagnitude لأنها أسرع في الحساب من magnitude
        bool isMoving = agent.velocity.sqrMagnitude > 0.1f;
        
        animator.SetBool(moveParameter, isMoving);
    }

    // دالة يستدعيها كود الهجوم
    public void PlayAttack()
    {
        if (animator != null)
        {
            animator.SetTrigger(attackParameter);
            // Debug.Log($"🎬 Animation: Attack Triggered for {name}");
        }
    }
}
