using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FormationGhost : PlacementGhost
{
    public float moveSpeed = 2f;    
    public float duration = 1.5f; 
    private TextMeshProUGUI textMesh;
    private Color startColor;
    private float timer;

    public void Setup(GameObject prefab, List<Vector3> offsets, Material mat)
    {
        foreach (var offset in offsets)
        {
            GameObject child = Instantiate(prefab, transform);
            child.transform.localPosition = offset;
            //  
            Destroy(child.GetComponent<UnityEngine.AI.NavMeshAgent>());
        }
        base.Initialize(mat);
    }

    void Start()
    {
        textMesh = GetComponentInChildren<TextMeshProUGUI>();
        if (textMesh != null)
        {
            startColor = textMesh.color;
        }
        Destroy(gameObject, duration);
    }

    void Update()
    {
        transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);

        transform.LookAt(transform.position + Camera.main.transform.forward);

        timer += Time.deltaTime;
        if (textMesh != null)
        {
            float alpha = Mathf.Lerp(1, 0, timer / duration);
            textMesh.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
        }
    }

}