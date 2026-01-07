using UnityEngine;
using UnityEngine.UI;

public class Castle : BuildingBase
{
    [Header("Castle Stats")]
    // Êã ÍĞİ currentHealth áÃäå ãæÌæÏ ÈÇáİÚá İí ÇáÃÈ (BuildingBase)
    public bool isPlayerCastle; // áÊÍÏíÏ åá åĞå ŞáÚÉ ÇááÇÚÈ Ãã ÇáÚÏæ

    protected override void Die()
    {
        if (isPlayerCastle)
        {
            Debug.Log("Game Over! áŞÏ ÏãÑÊ ŞáÚÊß.");
            // åäÇ íãßäß ÇÓÊÏÚÇÁ GameManager áÅäåÇÁ ÇááÚÈÉ
        }
        else
        {
            Debug.Log("Victory! áŞÏ ÏãÑÊ ŞáÚÉ ÇáÚÏæ.");
        }
        
        base.Die(); // ÊÏãíÑ ÇáßÇÆä
    }
}
