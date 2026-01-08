using UnityEngine;

public class BuildingGhost : MonoBehaviour
{
    private bool isOverlapping = false;
    private MeshRenderer[] renderers;

    void Start()
    {
        renderers = GetComponentsInChildren<MeshRenderer>();
    }
    public bool CanPlace()
    {
        return !isOverlapping;
    }

    void Update()
    {
        Collider[] colliders = Physics.OverlapBox(transform.position, new Vector3(1.5f, 1f, 1.5f), transform.rotation);

        isOverlapping = false;
        foreach (var col in colliders)
        {
            if (col.gameObject != this.gameObject && !col.CompareTag("Ground"))
            {
                isOverlapping = true;
                break;
            }
        }
        SetColor(CanPlace());
    }

    public void SetColor(bool isValid)
    {
        Color color = isValid ? new Color(0, 1, 0, 0.5f) : new Color(1, 0, 0, 0.5f);
        foreach (var r in renderers)
        {
            r.material.color = color;
        }
    }
}