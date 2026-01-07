using UnityEngine;

public class FloatingTextBillboard : MonoBehaviour
{
    void LateUpdate()
    {
        // íÌÚá ÇáßÇäİÇÓ íÏæÑ áíæÇÌå ÇáßÇãíÑÇ ãÈÇÔÑÉ
        transform.LookAt(transform.position + Camera.main.transform.forward);
    }
}