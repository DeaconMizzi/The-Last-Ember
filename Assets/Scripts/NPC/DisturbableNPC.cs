using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisturbableNPC : MonoBehaviour
{
    public string memoryFlagID = "angered_guard";
    public GameObject enemyPrefab;
    public float delayBeforeAttack = 1.2f;

    private bool hasBeenTriggered = false;

    public void TriggerAttack()
    {
        if (hasBeenTriggered) return;
        hasBeenTriggered = true;

        MemoryFlags.Set(memoryFlagID);
        DialogueManager.Instance.ForceEndDialogue();

        StartCoroutine(TransformToEnemy());
    }

    private System.Collections.IEnumerator TransformToEnemy()
    {
        yield return new WaitForSeconds(delayBeforeAttack);

        GameObject enemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity);

        StartCoroutine(SetBranhalmDefeatedAfter(enemy, 0.1f));

        Destroy(gameObject);
    }
    
    private IEnumerator SetBranhalmDefeatedAfter(GameObject enemy, float delay)
    {
        yield return new WaitForSeconds(delay);

        // Wait until the enemy is destroyed (meaning it died)
        yield return new WaitUntil(() => enemy == null);

        MemoryFlags.Set("BRANHALM_DEFEATED");
    }
}
