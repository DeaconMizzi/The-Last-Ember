using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrcSpawner : MonoBehaviour
{
    public GameObject orcPrefab;
    public Transform[] spawnPoints;
    public string triggerQuestID = "killorcs";
    public int orcsToSpawn = 5;

    private bool hasSpawned = false;

    void Start()
    {
        QuestManager.Instance.OnQuestGiven += TrySpawnOrcs;
    }

    void TrySpawnOrcs(string questID)
    {
        if (hasSpawned || questID != triggerQuestID)
            return;

        hasSpawned = true;

        for (int i = 0; i < orcsToSpawn && i < spawnPoints.Length; i++)
        {
            Instantiate(orcPrefab, spawnPoints[i].position, Quaternion.identity);
        }
    }

    void OnDestroy()
    {
        if (QuestManager.Instance != null)
            QuestManager.Instance.OnQuestGiven -= TrySpawnOrcs;
    }
}