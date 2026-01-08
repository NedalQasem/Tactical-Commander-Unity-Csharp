using System.Collections.Generic;
using UnityEngine;

public class FormationManager : MonoBehaviour
{
    public static FormationManager Instance;

    public enum FormationType { Line, Grid, Circle, Wedge }
    
    [Header("Settings")]
    public FormationType currentFormation = FormationType.Line;
    public float unitSpacing = 2.0f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        // Handle Shortcuts
        if (Input.GetKeyDown(KeyCode.Alpha1)) { currentFormation = FormationType.Line; Debug.Log("Formation: Line"); }
        if (Input.GetKeyDown(KeyCode.Alpha2)) { currentFormation = FormationType.Grid; Debug.Log("Formation: Grid"); } // Square
        if (Input.GetKeyDown(KeyCode.Alpha3)) { currentFormation = FormationType.Wedge; Debug.Log("Formation: Wedge"); } // Wedge
    }

    // --- UI Button Methods ---
    public void SetFormationLine() { currentFormation = FormationType.Line; }
    public void SetFormationSquare() { currentFormation = FormationType.Grid; }
    public void SetFormationWedge() { currentFormation = FormationType.Wedge; }
    public void SetFormationCircle() { currentFormation = FormationType.Circle; }

    public List<Vector3> GetFormationPositions(Vector3 center, int count, Quaternion rotation)
    {
        List<Vector3> points = new List<Vector3>();
        
        switch (currentFormation)
        {
            case FormationType.Line:
                float lineStart = -((count - 1) * unitSpacing) / 2f;
                for (int i = 0; i < count; i++)
                {
                    Vector3 offset = new Vector3(lineStart + i * unitSpacing, 0, 0);
                    points.Add(center + rotation * offset);
                }
                break;

            case FormationType.Grid:
                int cols = Mathf.CeilToInt(Mathf.Sqrt(count));
                float offsetVal = ((cols - 1) * unitSpacing) / 2f;
                for (int i = 0; i < count; i++)
                {
                    float x = (i % cols) * unitSpacing - offsetVal;
                    float z = (i / cols) * unitSpacing - offsetVal;
                    // -z لأننا نبني للخلف
                    Vector3 offset = new Vector3(x, 0, -z);
                    points.Add(center + rotation * offset);
                }
                break;

            case FormationType.Wedge:
                for (int i = 0; i < count; i++)
                {
                    if (i == 0) points.Add(center);
                    else
                    {
                        int row = (i + 1) / 2;
                        float xOffset = (i % 2 == 0 ? 1 : -1) * row * unitSpacing;
                        float zOffset = -row * unitSpacing;
                        Vector3 offset = new Vector3(xOffset, 0, zOffset);
                        points.Add(center + rotation * offset);
                    }
                }
                break;

            case FormationType.Circle:
                for (int i = 0; i < count; i++)
                {
                    float angle = i * (360f / count);
                    float radius = count * 0.5f; 
                    Vector3 pos = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad)) * radius;
                    points.Add(center + rotation * pos);
                }
                break;
        }
        return points;
    }
}
