using UnityEngine;
using UnityEngine.AI;

public class UnitAnimation : MonoBehaviour
{
    private Animator animator;
    private NavMeshAgent agent;

    [Header("Animation Parameter Names")]
    [SerializeField] private string moveParameter = "IsMoved";
    [SerializeField] private string attackParameter = "Attack";

    private void Start()
    {
        // البحث عن المكونات بذكاء (سواء على نفس الكائن أو الأبناء)
        animator = GetComponentInChildren<Animator>();
        if (animator == null) animator = GetComponent<Animator>();

        agent = GetComponent<NavMeshAgent>();

        if (animator == null) Debug.LogError($"❌ UnitAnimation: No Animator found on {transform.name}!");
        if (agent == null) Debug.LogError($"❌ UnitAnimation: No NavMeshAgent found on {transform.name}!");
    }

    private void Update()
    {
        if (animator == null || agent == null) return;

        bool isMoving = !agent.isStopped && agent.velocity.sqrMagnitude > 0.1f && agent.remainingDistance > agent.stoppingDistance;
        
        animator.SetBool(moveParameter, isMoving);
    }

    public void PlayAttack()
    {
        if (animator != null)
        {
            animator.SetTrigger(attackParameter);
            // Debug.Log($"🎬 Animation: Attack Triggered for {name}");
        }
    }
}
