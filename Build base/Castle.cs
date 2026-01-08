using UnityEngine;
using UnityEngine.UI;

public class Castle : BuildingBase
{
    [Header("Castle Stats")]
    public bool isPlayerCastle; 
    protected override void Die()
    {
        if (isPlayerCastle)
        {
            Debug.Log("Game Over!");
        }
        else
        {
            Debug.Log("Victory!");
        }
        base.Die();
    }
}
