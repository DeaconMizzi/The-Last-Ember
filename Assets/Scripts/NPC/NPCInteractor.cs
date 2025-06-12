using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCInteractor : MonoBehaviour
{
    public float interactionRadius = 1.5f;
    public LayerMask npcLayer;

    void Update()
    {
        if (!GameStateController.IsCutscenePlaying && !GameStateController.IsDialogueActive && Input.GetKeyDown(KeyCode.E))
        {
            Collider2D npcCollider = Physics2D.OverlapCircle(transform.position, interactionRadius, npcLayer);
            if (npcCollider != null)
            {
                NPC npc = npcCollider.GetComponent<NPC>();
                if (npc != null)
                {
                    DialogueManager.Instance.StartDialogue(npc);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}