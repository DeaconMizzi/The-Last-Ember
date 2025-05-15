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

        Instantiate(enemyPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
