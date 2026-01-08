using System.Collections.Generic;
using UnityEngine;

public class Barracks : BuildingBase
{
    [Header("Training Settings")]
    public Transform spawnPoint;
    public Transform rallyPoint;
    public GameObject visualRallyPoint;

    private Queue<UnitData> trainingQueue = new Queue<UnitData>();
    private float trainingTimer = 0f;
    private bool isTraining = false;
    private UnitData currentUnit;

    void Update()
    {
        if (!isPlaced) return;

        if (isTraining && currentUnit != null)
        {
            trainingTimer += Time.deltaTime;
            if (trainingTimer >= currentUnit.trainingTime)
            {
                SpawnUnit();
                // Check if queue has more
                if (trainingQueue.Count > 0)
                {
                    StartTrainingNext();
                }
                else
                {
                    isTraining = false;
                    currentUnit = null;
                    trainingTimer = 0f;
                }
            }
        }
    }

    public void EnqueueUnit(UnitData unitData)
    {
        trainingQueue.Enqueue(unitData);
        if (!isTraining)
        {
            StartTrainingNext();
        }
    }

    void StartTrainingNext()
    {
        if (trainingQueue.Count > 0)
        {
            currentUnit = trainingQueue.Dequeue();
            trainingTimer = 0f;
            isTraining = true;
        }
    }

    void SpawnUnit()
    {
        if (currentUnit != null && currentUnit.unitPrefab != null)
        {
            GameObject newUnit = Instantiate(currentUnit.unitPrefab, spawnPoint.position, spawnPoint.rotation);
            Unit unitScript = newUnit.GetComponent<Unit>();
            if (unitScript != null)
            {
                unitScript.data = currentUnit;
                // Move to rally point if set
                if (rallyPoint != null)
                {
                    unitScript.MoveTo(rallyPoint.position);
                }
                
                if (AudioManager.Instance != null) AudioManager.Instance.PlaySFXAt(SoundType.UnitSpawn, spawnPoint.position);
            }
        }
    }

    public float GetTrainingProgress(UnitData data)
    {
        if (isTraining && currentUnit == data)
        {
            return 1f - (trainingTimer / currentUnit.trainingTime);
        }
        return 0f;
    }

    public int GetQueueCount(UnitData data)
    {
        int count = 0;
        foreach (var u in trainingQueue)
        {
            if (u == data) count++;
        }
        if (isTraining && currentUnit == data) count++;
        return count;
    }
}
