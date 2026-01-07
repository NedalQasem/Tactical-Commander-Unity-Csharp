using UnityEngine;

public class Billboard : MonoBehaviour
{
    void LateUpdate()
    {
        // íÌÚá ÇáÔÑíØ íæÇÌå ÇáßÇãíÑÇ ÏÇÆãÇğ áÊÓåíá ÇáŞÑÇÁÉ
        transform.LookAt(transform.position + Camera.main.transform.forward);
    }
}