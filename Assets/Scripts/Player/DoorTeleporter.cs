using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorTeleporter : MonoBehaviour
{
    public Transform teleportDestination;
    public KeyCode interactKey = KeyCode.E;
    public float interactRange = 1.5f;
    public GameObject player;

    [Header("Optional: Map Transition")]
    public MapTransition mapTransition;

    private void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(player.transform.position, transform.position);
        if (distance <= interactRange && Input.GetKeyDown(interactKey))
        {
            if (teleportDestination != null)
            {
                player.transform.position = teleportDestination.position;
                Debug.Log("[DoorTeleporter] Teleported player.");

                // Manually invoke MapTransition logic
                if (mapTransition != null)
                {
                    mapTransition.ForceTransition(player);
                }
            }
        }
    }
}
