using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaperInteractable : MonoBehaviour
{
    [TextArea] public string[] paperLines;
    public string title = "Note";
    public Sprite icon;
    public GameObject talkPrompt;

    [Header("Flags")]
    public string memoryFlagID = "note_memory_key";
    public bool hideAfterReading = true;

    [Header("Interaction")]
    public float interactRange = 2f;

    private Transform player;
    private bool promptShown = false;
    private bool hasBeenRead = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (talkPrompt != null)
            talkPrompt.SetActive(false);

        if (!string.IsNullOrEmpty(memoryFlagID) && MemoryFlags.Get(memoryFlagID))
        {
            if (hideAfterReading)
            {
                hasBeenRead = true;
                gameObject.SetActive(false);
            }
        }
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= interactRange)
        {
            if (!promptShown && talkPrompt != null)
            {
                talkPrompt.SetActive(true);
                promptShown = true;
            }

            if (!hasBeenRead && Input.GetKeyDown(KeyCode.E))
            {
                DialogueManager.Instance.StartSimpleDialogue(paperLines, title, icon, OnNoteRead);
            }
            else if (hasBeenRead && !hideAfterReading && Input.GetKeyDown(KeyCode.E))
            {
                // Allow re-reading if note is not hidden
                DialogueManager.Instance.StartSimpleDialogue(paperLines, title, icon, null);
            }
        }
        else
        {
            if (promptShown && talkPrompt != null)
            {
                talkPrompt.SetActive(false);
                promptShown = false;
            }
        }
    }

    void OnNoteRead()
    {
        if (!string.IsNullOrEmpty(memoryFlagID))
        {
            MemoryFlags.Set(memoryFlagID);
        }

        QuestLogUI.Instance?.ShowQuestPopup("Note added to journal");

        if (hideAfterReading)
        {
            hasBeenRead = true;
            gameObject.SetActive(false);
        }
    }
}
