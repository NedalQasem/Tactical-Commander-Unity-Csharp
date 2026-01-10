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

    protected override void Awake()
    {
        base.Awake();
        
        // 🛠️ Auto-Fix: If user assigned a Prefab to visualRallyPoint, instantiate it!
        if (visualRallyPoint != null && !visualRallyPoint.scene.IsValid())
        {
            GameObject prefab = visualRallyPoint;
            visualRallyPoint = Instantiate(prefab, transform.position, Quaternion.identity);
            visualRallyPoint.name = $"{buildingName}_RallyPoint";
            visualRallyPoint.SetActive(false);
            
            // If rallyPoint was pointing to the prefab's transform, update it
            if (rallyPoint != null && !rallyPoint.gameObject.scene.IsValid())
            {
                rallyPoint = visualRallyPoint.transform;
            }
        }
    }

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
                unitScript.team = this.team; // Ensure team inheritance

                // Default Rally Point Logic
                Vector3 targetPos = spawnPoint.position + spawnPoint.forward * 5f;
                if (rallyPoint != null) targetPos = rallyPoint.position;

                // Move after a short delay to ensure NavMeshAgent is bound
                StartCoroutine(MoveUnitAfterSpawn(unitScript, targetPos));
                
                if (AudioManager.Instance != null) AudioManager.Instance.PlaySFXAt(SoundType.UnitSpawn, spawnPoint.position);
            }
        }
    }

    private System.Collections.IEnumerator MoveUnitAfterSpawn(Unit unit, Vector3 destination)
    {
        // Wait for NavMeshAgent to initialize
        yield return new WaitForEndOfFrame(); 

        if (unit != null && unit.agent != null)
        {
            // Ensure unit is on NavMesh
            UnityEngine.AI.NavMeshHit hit;
            if (UnityEngine.AI.NavMesh.SamplePosition(unit.transform.position, out hit, 2.0f, UnityEngine.AI.NavMesh.AllAreas))
            {
                unit.agent.Warp(hit.position);
            }
            
            // Wait one more physics frame for Warp to take effect
            yield return new WaitForFixedUpdate();

            if (unit.IsAgentReady)
            {
                unit.MoveTo(destination);
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
