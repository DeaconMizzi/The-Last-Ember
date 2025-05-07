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

    public Image portraitImage;

    public GameObject choicePanel; // Panel holding buttons
    public GameObject choiceButtonPrefab; // A UI Button prefab

    private DialogueNode currentNode;

    private void Awake()
    {
        Instance = this;
        dialoguePanel.SetActive(false);
    }

    public void StartDialogue(NPC npc)
    {
        if (npc.startingNode == null)
        {
            Debug.LogWarning("NPC has no starting dialogue node assigned.");
            return;
        }

        currentNode = npc.startingNode;
        dialoguePanel.SetActive(true);
        DisplayNode(currentNode);
    }

    public void DisplayNode(DialogueNode node)
    {
        ClearChoices();

        speakerText.text = node.speakerName;
        dialogueText.text = node.dialogueText;

        if (portraitImage != null)
        {
            if (node.portrait != null)
            {
                portraitImage.sprite = node.portrait;
                portraitImage.enabled = true;
            }
            else
            {
                portraitImage.enabled = false;
            }
        }

        foreach (DialogueChoice choice in node.choices)
        {
            GameObject choiceObj = Instantiate(choiceButtonPrefab, choicePanel.transform);
            TMP_Text choiceText = choiceObj.GetComponentInChildren<TMP_Text>();
            choiceText.text = choice.choiceText;

            choiceObj.GetComponent<Button>().onClick.AddListener(() => OnChoiceSelected(choice));
        }
        
        if (node.choices == null || node.choices.Count == 0)
        {
            // No choices? Advance automatically after key press
             StartCoroutine(WaitAndAdvance(node));
         }
    }

    IEnumerator WaitAndAdvance(DialogueNode current)
{
    yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));

    if (current.nextAutoNode != null)
    {
        DisplayNode(current.nextAutoNode);
    }
    else
    {
        EndDialogue();
    }
}

    void OnChoiceSelected(DialogueChoice choice)
    {
        if (!string.IsNullOrEmpty(choice.consequenceID))
        {
            PlayerPrefs.SetInt(choice.consequenceID, 1); // Or your custom save manager
        }

        if (choice.nextNode != null)
        {
            currentNode = choice.nextNode;
            DisplayNode(currentNode);
        }
        else
        {
            EndDialogue();
        }
    }

    void ClearChoices()
    {
        foreach (Transform child in choicePanel.transform)
        {
            Destroy(child.gameObject);
        }
    }

    void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        ClearChoices();
    }
}
