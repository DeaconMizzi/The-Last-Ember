using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaperInteractable : MonoBehaviour
{
    public string[] paperLines;
    public string title = "Note";
    public Sprite icon;
    public GameObject talkPrompt;

    public float interactRange = 2f;
    private Transform player;
    private bool promptShown = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (talkPrompt != null)
            talkPrompt.SetActive(false);
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

            if (Input.GetKeyDown(KeyCode.E))
            {
                DialogueManager.Instance.StartSimpleDialogue(paperLines, title, icon);
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
}
