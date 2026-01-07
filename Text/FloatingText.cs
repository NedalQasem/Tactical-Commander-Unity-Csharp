using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    public float destroyTime = 1.5f;
    void Start()
    {
        Destroy(gameObject, destroyTime);
    }

}