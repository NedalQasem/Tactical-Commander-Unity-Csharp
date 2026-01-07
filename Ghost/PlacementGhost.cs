using UnityEngine;

public class PlacementGhost : MonoBehaviour
{
    protected MeshRenderer[] renderers;

    [Header("Detection Settings")]
    public Vector3 boxSize = new Vector3(1.5f, 1f, 1.5f); // 🛑 تأكد من تعريفها هنا
    public virtual void Initialize(Material ghostMat)
    {
        renderers = GetComponentsInChildren<MeshRenderer>();
        foreach (var r in renderers) r.material = ghostMat;
    }

    // 🛑 هذه هي الدالة التي يطلبها الـ PlacementManager في صورتك
    public virtual bool CanPlace()
    {
        // التحقق من التصادم في موقع الشبح الحالي
        Collider[] colliders = Physics.OverlapBox(transform.position, boxSize / 2, transform.rotation);

        foreach (var col in colliders)
        {
            // إذا لمسنا أي شيء ليس الأرض (تأكد أن الأرض Tag بتاعها Ground)
            if (col.gameObject != this.gameObject && !col.CompareTag("Ground"))
            {
                return false; // لا يمكن البناء هنا
            }
        }
        return true; // المكان فارغ ومناسب
    }

    public virtual void SetColor(bool isValid)
    {
        Color color = isValid ? new Color(0, 1, 0, 0.5f) : new Color(1, 0, 0, 0.5f);
        if (renderers == null) renderers = GetComponentsInChildren<MeshRenderer>();
        foreach (var r in renderers) r.material.color = color;
    }
}