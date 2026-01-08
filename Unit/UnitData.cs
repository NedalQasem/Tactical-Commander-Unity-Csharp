using UnityEngine;

// هذا السطر يسمح لك بإنشاء الملف من قائمة Create في يونتي
[CreateAssetMenu(fileName = "NewUnitData", menuName = "RTS/Unit Data")]
public class UnitData : ScriptableObject
{
    [Header("Visuals")]
    public string unitName;
    public Sprite unitIcon; // أيقونة الجندي للثكنة
    public GameObject unitPrefab; // الموديل الخاص بالجندي

    [Header("Economy")]
    public int goldCost; // سعر الجندي
    public float trainingTime; // زمن التدريب في الثكنة

    [Header("Stats")]
    public int maxHealth;
    public int attackDamage;
    public float moveSpeed;
    public enum AttackType { Melee, Ranged }
    
    [Header("Combat Config")]
    public AttackType attackType = AttackType.Melee;
    public GameObject projectilePrefab; // للرماة فقط: ضع سهم هنا
    public float attackRange;
    public float attackRate = 1.0f; // سرعة الهجوم (بالثواني بين الضربات)
    public float visionRange = 8.0f; // مدى الرؤية للاستهداف التلقائي

    [Header("Balancing")]
    public float damageMultiplier = 1.0f; // لتسهيل موازنة الضرر لاحقاً
}