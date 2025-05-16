using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowerTrailSpawner : MonoBehaviour
{
    public GameObject flowerPrefab;
    public float spawnInterval = 0.1f;
    public float lifetime = 0.75f;

    private bool isBloomstepping = false;

    public void StartTrail(float duration)
    {
        if (!isBloomstepping)
            StartCoroutine(SpawnTrail(duration));
    }

    System.Collections.IEnumerator SpawnTrail(float duration)
    {
        isBloomstepping = true;
        float timeElapsed = 0f;

        while (timeElapsed < duration)
        {
            GameObject flower = Instantiate(flowerPrefab, transform.position, Quaternion.identity);
            Destroy(flower, lifetime);
            yield return new WaitForSeconds(spawnInterval);
            timeElapsed += spawnInterval;
        }

        isBloomstepping = false;
    }
}