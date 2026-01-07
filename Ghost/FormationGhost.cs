using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FormationGhost : PlacementGhost
{
    public float moveSpeed = 2f;    // ”—⁄… «·’⁄Êœ
    public float duration = 1.5f;   // „œ… «·»ﬁ«¡
    private TextMeshProUGUI textMesh;
    private Color startColor;
    private float timer;

    public void Setup(GameObject prefab, List<Vector3> offsets, Material mat)
    {
        void Start()
        {
            textMesh = GetComponentInChildren<TextMeshProUGUI>();
            if (textMesh != null)
            {
                startColor = textMesh.color;
            }

            //  œ„Ì— «·ﬂ«∆‰ »«·ﬂ«„· „‰ «·–«ﬂ—… »⁄œ «‰ Â«¡ «·Êﬁ 
            Destroy(gameObject, duration);
        }

        foreach (var offset in offsets)
        {
            GameObject child = Instantiate(prefab, transform);
            child.transform.localPosition = offset;
            //  ‰ŸÌ› «·‰”Œ…
            Destroy(child.GetComponent<UnityEngine.AI.NavMeshAgent>());
        }
        base.Initialize(mat);
    }

    void Update()
    {
        // 1. «· Õ—Ìﬂ ··√⁄·Ï
        transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);

        // 2. Ã⁄· «·‰’ ÌÊ«ÃÂ «·ﬂ«„Ì—« œ«∆„«
        transform.LookAt(transform.position + Camera.main.transform.forward);

        // 3.  √ÀÌ— «· ·«‘Ì ( €ÌÌ— «·‘›«›Ì… Alpha)
        timer += Time.deltaTime;
        if (textMesh != null)
        {
            float alpha = Mathf.Lerp(1, 0, timer / duration);
            textMesh.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
        }
    }

}