using UnityEngine;

public class GoldMine : BuildingBase
{
    [Header("Revenue Settings")]
    public int goldAmount = 10;
    public float interval = 5f;

    private float timer;
    [Header("VFX Settings")]
    public GameObject floatingTextPrefab;

    void Start()
    {
        timer = interval;
    }

    void Update()
    {
        if (!isPlaced) return;

        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            GenerateGold();
            timer = interval;
        }
    }

    void GenerateGold()
    {
        if (team == Unit.Team.Player)
        {
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.AddGold(goldAmount);

                if (floatingTextPrefab != null)
                {
                    Vector3 spawnPos = transform.position + Vector3.up * 2.5f; 
                    Instantiate(floatingTextPrefab, spawnPos, Quaternion.identity);
                }
            }
        }
        else
        {
           
        }
    }
}
