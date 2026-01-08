using UnityEngine;

public interface IDamageable
{
    Unit.Team GetTeam();
    Transform GetTransform();
    void TakeDamage(int amount);
    bool IsAlive();
    float GetRadius(); 
    Collider GetCollider();
}
