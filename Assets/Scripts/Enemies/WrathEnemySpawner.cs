using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WrathEnemySpawner : MonoBehaviour
{
    public static WrathEnemySpawner Instance;

    [Header("Enemy Prefabs (Random Selection)")]
    public GameObject enemyType1;
    public GameObject enemyType2;

    [Header("Spawn Points (Children of this object)")]
    public List<Transform> spawnPoints = new List<Transform>();

    private bool hasSpawned = false;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // Auto-fill spawn points from children
        foreach (Transform child in transform)
        {
            spawnPoints.Add(child);
        }
    }

    public void TriggerSpawns()
    {
        if (hasSpawned)
            return;

        if (enemyType1 == null || enemyType2 == null)
        {
            Debug.LogWarning("WrathEnemySpawner is missing enemy prefabs.");
            return;
        }

        foreach (Transform point in spawnPoints)
        {
            GameObject selected = Random.value < 0.5f ? enemyType1 : enemyType2;
            Instantiate(selected, point.position, Quaternion.identity);
        }

        hasSpawned = true;
        Debug.Log("🔥 Wrath enemies have been unleashed.");
    }
}