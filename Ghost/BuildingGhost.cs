using UnityEngine;

public class BuildingGhost : MonoBehaviour
{
    private bool isOverlapping = false;
    private MeshRenderer[] renderers;

    void Start()
    {
        renderers = GetComponentsInChildren<MeshRenderer>();
    }

    // Â–Â «·œ«·… ” Œ»—‰« ≈–« ﬂ«‰ «·„ﬂ«‰ „ÕÃÊ“«
    public bool CanPlace()
    {
        return !isOverlapping;
    }

    void Update()
    {
        // «” Œœ«„ OverlapBox ·· √ﬂœ „‰ Œ·Ê «·„‰ÿﬁ…
        // ‰” Œœ„ ‰›” ÕÃ„ «·‹ Collider «·Œ«’ »«·„»‰Ï
        Collider[] colliders = Physics.OverlapBox(transform.position, new Vector3(1.5f, 1f, 1.5f), transform.rotation);

        isOverlapping = false;
        foreach (var col in colliders)
        {
            // ≈–« ·„”‰« √Ì ‘Ì¡ ·Ì” "«·√—÷"
            if (col.gameObject != this.gameObject && !col.CompareTag("Ground"))
            {
                isOverlapping = true;
                break;
            }
        }

        //  €ÌÌ— «··Ê‰ »‰«¡ ⁄·Ï ’·«ÕÌ… «·„ﬂ«‰
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