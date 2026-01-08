using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    public float destroyTime = 1.5f;
    public float moveSpeed = 2f;
    public Vector3 offset = new Vector3(0, 2, 0);

    private TMP_Text textMesh;
    private Color startColor;

    void Start()
    {
        textMesh = GetComponent<TMP_Text>();
        if (textMesh != null) startColor = textMesh.color;
        
        transform.position += new Vector3(Random.Range(-0.5f, 0.5f), 0, 0);
        
        Destroy(gameObject, destroyTime);
    }

    void Update()
    {
        // 1. Move Up
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        // 2. Fade Out
        if (textMesh != null)
        {
            float alpha = textMesh.color.a - (Time.deltaTime / destroyTime);
            textMesh.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
        }
    }

}