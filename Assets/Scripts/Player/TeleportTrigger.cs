using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportTrigger : MonoBehaviour
{
    public Transform teleportTarget;
    public KeyCode interactKey = KeyCode.E;

    private bool playerInZone = false;

    void Update()
    {
        if (playerInZone && Input.GetKeyDown(interactKey))
        {
            StartCoroutine(FadeAndTeleport());
        }
    }

    IEnumerator FadeAndTeleport()
    {
        yield return FadeController.Instance.FadeOutIn(() =>
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null && teleportTarget != null)
                player.transform.position = teleportTarget.position;
        });
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInZone = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInZone = false;
    }
}