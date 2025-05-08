using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;
    public static bool DialogueIsOpen => Instance != null && Instance.dialoguePanel.activeSelf;

    public GameObject dialoguePanel;
    public TMP_Text speakerText;
    public TMP_Text dialogueText;
    public Image portraitImage;

    public GameObject choicePanel;
    public GameObject choiceSlotPrefab;
    public int visibleChoiceCount = 3;

    private DialogueNode currentNode;
    private List<DialogueChoice> currentChoices = new List<DialogueChoice>();
    private List<GameObject> instantiatedSlots = new List<GameObject>();

    private int selectedIndex = 0;
    private int scrollOffset = 0;
    private bool isWaitingForContinue = false;
    private Coroutine waitCoroutine = null;
    public float typingSpeed = 0.03f;
    private Coroutine typingCoroutine = null;

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
        currentNode = node;
        speakerText.text = node.speakerName;

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(node.dialogueText));

        if (portraitImage != null)
        {
            portraitImage.sprite = node.portrait;
            portraitImage.enabled = node.portrait != null;
        }

        currentChoices = node.choices;
        selectedIndex = 0;
        scrollOffset = 0;

        ClearChoices();
        isWaitingForContinue = false;

        if (waitCoroutine != null)
        {
            StopCoroutine(waitCoroutine);
            waitCoroutine = null;
        }

        // Don't start WaitToContinue here — let TypeText handle that
    }

    void ClearChoices()
    {
        foreach (GameObject slot in instantiatedSlots)
            Destroy(slot);
        instantiatedSlots.Clear();
    }

    void RenderChoices()
    {
        ClearChoices();

        int max = Mathf.Min(visibleChoiceCount, currentChoices.Count - scrollOffset);

        for (int i = 0; i < max; i++)
        {
            int choiceIndex = scrollOffset + i;
            GameObject slot = Instantiate(choiceSlotPrefab, choicePanel.transform);
            instantiatedSlots.Add(slot);

            TMP_Text choiceText = slot.transform.Find("ChoiceText")?.GetComponent<TMP_Text>();
            Image arrow = slot.transform.Find("Arrow")?.GetComponent<Image>();
            Image bg = slot.transform.Find("Background")?.GetComponent<Image>();

            if (choiceText != null)
                choiceText.text = currentChoices[choiceIndex].choiceText;

            bool isSelected = (choiceIndex == selectedIndex);
            if (arrow != null)
                arrow.gameObject.SetActive(isSelected);
            if (bg != null)
                bg.enabled = isSelected;
        }
    }

    IEnumerator WaitToContinue()
    {
        Debug.Log("Waiting for V to continue from: " + currentNode.name);
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.V));
        yield return new WaitUntil(() => Input.GetKeyUp(KeyCode.V));

        isWaitingForContinue = false;

        if (currentNode.nextAutoNode != null)
        {
            Debug.Log("Advancing to: " + currentNode.nextAutoNode.name);
            DisplayNode(currentNode.nextAutoNode);
        }
        else
        {
            Debug.Log("No next node. Ending dialogue.");
            EndDialogue();
        }

        waitCoroutine = null;
    }

    void Update()
    {
        if (!dialoguePanel.activeSelf)
            return;

        // If typing is still ongoing, V finishes typing only
        if (Input.GetKeyDown(KeyCode.V) && typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            dialogueText.text = currentNode.dialogueText;
            typingCoroutine = null;

            if (currentChoices == null || currentChoices.Count == 0)
            {
                if (waitCoroutine != null)
                    StopCoroutine(waitCoroutine);
                waitCoroutine = StartCoroutine(WaitToContinue());
                isWaitingForContinue = true;
            }
            else
            {
                RenderChoices();
            }

            return;
        }

        if (isWaitingForContinue || currentChoices == null || currentChoices.Count == 0)
            return;

        bool changed = false;

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            selectedIndex = Mathf.Min(selectedIndex + 1, currentChoices.Count - 1);
            if (selectedIndex >= scrollOffset + visibleChoiceCount)
                scrollOffset++;
            changed = true;
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            selectedIndex = Mathf.Max(selectedIndex - 1, 0);
            if (selectedIndex < scrollOffset)
                scrollOffset--;
            changed = true;
        }

        if (changed)
            RenderChoices();

        if (Input.GetKeyDown(KeyCode.Return))
        {
            DialogueChoice choice = currentChoices[selectedIndex];

            if (!string.IsNullOrEmpty(choice.consequenceID))
                PlayerPrefs.SetInt(choice.consequenceID, 1);

            if (choice.nextNode != null)
                DisplayNode(choice.nextNode);
            else
                EndDialogue();
        }
    }

    void EndDialogue()
    {
        if (waitCoroutine != null)
        {
            StopCoroutine(waitCoroutine);
            waitCoroutine = null;
        }

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        dialoguePanel.SetActive(false);
        ClearChoices();
        currentChoices.Clear();
    }

    IEnumerator TypeText(string fullText)
    {
        dialogueText.text = "";

        foreach (char c in fullText)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        typingCoroutine = null;

        if (currentChoices == null || currentChoices.Count == 0)
        {
            isWaitingForContinue = true;
            if (waitCoroutine != null)
                StopCoroutine(waitCoroutine);
            waitCoroutine = StartCoroutine(WaitToContinue());
        }
        else
        {
            RenderChoices();
        }
    }
}
