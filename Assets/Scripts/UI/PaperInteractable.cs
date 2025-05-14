using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaperInteractable : MonoBehaviour
{
    public string[] paperLines;
    public string title = "Note";
    public Sprite icon;
    public GameObject talkPrompt;

    public string memoryFlagID = "note_memory_key"; // Unique ID for this note
    public bool hideAfterReading = true;

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
            hasBeenRead = true;
            if (hideAfterReading)
                gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (player == null || hasBeenRead) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= interactRange)
        {
            if (!promptShown && talkPrompt != null)
            {
                talkPrompt.SetActive(true);
                promptShown = true;
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                DialogueManager.Instance.StartSimpleDialogue(paperLines, title, icon, OnNoteRead);
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

        hasBeenRead = true;

        if (hideAfterReading)
            gameObject.SetActive(false);
    }
}