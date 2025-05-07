using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    public GameObject dialoguePanel;
    public TMP_Text speakerText;
    public TMP_Text dialogueText;
    public float typingSpeed = 0.02f;

    private Queue<DialogueLine> linesQueue;
    private NPC currentNPC;
    public Image portraitImage;


    private void Awake()
    {
        Instance = this;
        linesQueue = new Queue<DialogueLine>();
        dialoguePanel.SetActive(false);
    }

    public void StartDialogue(NPC npc)
    {
        currentNPC = npc;
        linesQueue.Clear();

        foreach (DialogueLine line in npc.dialogue)
        {
            linesQueue.Enqueue(line);
        }

        dialoguePanel.SetActive(true);
        DisplayNextLine();
    }

    public void DisplayNextLine()
    {
        if (linesQueue.Count == 0)
        {
            EndDialogue();
            return;
        }

        DialogueLine line = linesQueue.Dequeue();
        StopAllCoroutines();
        StartCoroutine(TypeSentence(line));
    }

    IEnumerator TypeSentence(DialogueLine line)
    {
        speakerText.text = line.speakerName;
        dialogueText.text = "";

        if (line.portrait != null)
        {
            portraitImage.sprite = line.portrait;
            portraitImage.enabled = true;
        }
        else
        {
            portraitImage.enabled = false; // Hide if none
        }

        foreach (char letter in line.line.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

   void EndDialogue()
{
    dialoguePanel.SetActive(false);

    if (currentNPC != null)
    {
        // Only attempt to give a quest if this NPC actually has one
        if (currentNPC.quest != null && !currentNPC.questGiven)
        {
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.GiveQuest(currentNPC.quest);
                currentNPC.questGiven = true;
            }
            else
            {
                Debug.LogWarning("QuestManager.Instance is missing from the scene.");
            }
        }

        currentNPC = null; // Always clear after use
    }
}
    void Update()
{
    if (dialoguePanel.activeSelf && Input.GetKeyDown(KeyCode.V))
    {
        DisplayNextLine();
    }
}
}