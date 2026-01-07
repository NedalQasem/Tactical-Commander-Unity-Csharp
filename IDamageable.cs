using UnityEngine;

public interface IDamageable
{
    Unit.Team GetTeam();
    Transform GetTransform();
    void TakeDamage(int amount);
    bool IsAlive();
    float GetRadius(); // لحساب المسافة بشكل أدق (للمباني الكبيرة)
}
