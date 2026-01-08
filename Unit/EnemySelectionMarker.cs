using UnityEngine;

public class EnemySelectionMarker : MonoBehaviour
{
    private Unit unit;
    private SpriteRenderer circleRenderer;

    void Start()
    {
        unit = GetComponentInParent<Unit>();
        
        if (unit != null && unit.selectionCircle != null)
        {
            circleRenderer = unit.selectionCircle.GetComponent<SpriteRenderer>();
            if (circleRenderer == null) circleRenderer = unit.selectionCircle.GetComponentInChildren<SpriteRenderer>();
        }
        else
        {
            Debug.LogWarning("EnemySelectionMarker: Could not find Unit or SelectionCircle!");
            enabled = false;
        }

        // Apply immediately
        ApplyVisuals();
    }

    void Update()
    {
        // Continuously enforce visibility for Enemies
        if (unit != null && unit.team == Unit.Team.Enemy && unit.selectionCircle != null)
        {
            if (!unit.selectionCircle.activeSelf) 
            {
                unit.selectionCircle.SetActive(true);
            }
        }
    }

    void ApplyVisuals()
    {
        if (unit == null || unit.selectionCircle == null) return;

        if (unit.team == Unit.Team.Enemy)
        {
            // 🔴 Force Red & Visible
            unit.selectionCircle.SetActive(true);
            if (circleRenderer != null) circleRenderer.color = Color.red;
        }
    }
}
