using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TalkPromptController : MonoBehaviour
{
    public GameObject promptUI;
    private Transform player;
    private float displayDistance = 1.5f;
    private NPC npc;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        npc = GetComponent<NPC>();

        // If NPC is not talkable, disable prompt
        if (npc == null || npc.startingNode == null)
        {
            if (promptUI != null)
                promptUI.SetActive(false);

            enabled = false;
            return;
        }

        // Position prompt above the head based on BoxCollider2D height
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null && promptUI != null)
        {
            float height = col.size.y;
            Vector3 offset = new Vector3(0, height, 0); // adjust 0.05f as needed
            promptUI.transform.localPosition = offset;
        }

        promptUI.SetActive(false);
    }

    void Update()
    {
        if (promptUI == null) return;

        float dist = Vector2.Distance(player.position, transform.position);
        promptUI.SetActive(dist <= displayDistance);
    }
}
