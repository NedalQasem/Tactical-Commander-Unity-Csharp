using UnityEngine;

// هذا السطر يسمح لك بإنشاء الملف من قائمة Create في يونتي
[CreateAssetMenu(fileName = "NewUnitData", menuName = "RTS/Unit Data")]
public class UnitData : ScriptableObject
{
    [Header("Visuals")]
    public string unitName;
    public Sprite unitIcon; 
    public GameObject unitPrefab; 

    [Header("Economy")]
    public int goldCost;
    public float trainingTime; 

    [Header("Stats")]
    public int maxHealth;
    public int attackDamage;
    public float moveSpeed;
    public enum AttackType { Melee, Ranged }
    
    [Header("Combat Config")]
    public AttackType attackType = AttackType.Melee;
    public GameObject projectilePrefab; 
    public float attackRange;
    public float attackRate = 1.0f; 
    public float visionRange = 8.0f; 

    [Header("Balancing")]
    public float damageMultiplier = 1.0f;
}