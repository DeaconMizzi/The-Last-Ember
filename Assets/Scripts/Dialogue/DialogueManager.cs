using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

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
    public GameObject choicePanelBackground;

    private DialogueNode currentNode;
    private List<DialogueChoice> currentChoices = new List<DialogueChoice>();
    private List<GameObject> instantiatedSlots = new List<GameObject>();
    public GameObject portraitBackground;

    private int selectedIndex = 0;
    private int scrollOffset = 0;
    private bool isWaitingForContinue = false;
    private Coroutine waitCoroutine = null;
    public float typingSpeed = 0.03f;
    private Coroutine typingCoroutine = null;

    private NPC currentNPC;
    public GridOvergrowthManager overgrowthManager;
    [Header("Audio")]
    public AudioSource typingAudioSource;

    private void Awake()
    {
        Instance = this;
        dialoguePanel.SetActive(false);
    }

    public void StartDialogue(NPC npc)
    {
        if (GameStateController.IsCutscenePlaying)
        {
            Debug.Log("Dialogue start prevented during cutscene.");
            return;
        }

        if (GameStateController.IsDialogueActive)
        {
            Debug.Log("Dialogue already active.");
            return;
        }

        GameStateController.Instance?.SetDialogueState(true);
        currentNPC = npc;

        if (npc == null)
        {
            Debug.LogWarning("No NPC passed to StartDialogue.");
            return;
        }

        // If quest was completed before and they should be silent
        if (npc.assignedQuest != null && npc.assignedQuest.hasCompletedQuest && !npc.canTalkAfterQuest)
        {
            DialogueNode silentNode = CreateFallbackNode(npc.npcName, "They look busy and don’t respond.");
            dialoguePanel.SetActive(true);
            DisplayNode(silentNode);
            return;
        }

        // If a runtime quest is active and completed
        if (npc.assignedQuest != null && QuestManager.Instance.IsQuestComplete(npc.assignedQuest.questID))
        {
            // One-time conditional dialogue override
            bool hasTriggerFlag = !string.IsNullOrEmpty(npc.memoryFlagCondition) && MemoryFlags.Get(npc.memoryFlagCondition);
            bool hasUsedFlag = !string.IsNullOrEmpty(npc.conditionalUsedFlagID) && MemoryFlags.Get(npc.conditionalUsedFlagID);

            if (hasTriggerFlag && !hasUsedFlag && npc.conditionalStartingNode != null)
            {
                currentNode = npc.conditionalStartingNode;
                if (!string.IsNullOrEmpty(npc.conditionalUsedFlagID))
                    MemoryFlags.Set(npc.conditionalUsedFlagID);
            }
            else if (npc.questCompleteNode != null)
            {
                Debug.Log(" Showing Elder’s post-ember dialogue.");
                currentNode = npc.questCompleteNode;
            }
            else
            {
                Debug.Log("❗ Elder has no questCompleteNode fallback.");
                currentNode = CreateFallbackNode(npc.npcName, "They look thoughtful… watching the flames beyond.");
            }

            QuestManager.Instance.RemoveQuest(npc.assignedQuest.questID);
            npc.activeRuntimeQuest.questGiven = false;
            npc.activeRuntimeQuest.isComplete = false;
            npc.assignedQuest.hasCompletedQuest = true;

            StartCoroutine(DelayedQuestUIUpdate());

            dialoguePanel.SetActive(true);
            DisplayNode(currentNode);
            return;
        }

        // If quest is active but incomplete
        if (npc.activeRuntimeQuest != null && npc.activeRuntimeQuest.questGiven)
        {
            if (npc.questAcceptedNode != null)
            {
                currentNode = npc.questAcceptedNode;
            }
            else
            {
                currentNode = CreateFallbackNode(npc.npcName, "They don’t seem like they want to talk right now.");
            }
        }
        else
        {
            // One-time conditional dialogue check when no quest is active
            bool hasTriggerFlag = !string.IsNullOrEmpty(npc.memoryFlagCondition) && MemoryFlags.Get(npc.memoryFlagCondition);
            bool hasUsedFlag = !string.IsNullOrEmpty(npc.conditionalUsedFlagID) && MemoryFlags.Get(npc.conditionalUsedFlagID);

            if (hasTriggerFlag && !hasUsedFlag && npc.conditionalStartingNode != null)
            {
                currentNode = npc.conditionalStartingNode;
                if (!string.IsNullOrEmpty(npc.conditionalUsedFlagID))
                    MemoryFlags.Set(npc.conditionalUsedFlagID);
            }
            else
            {
                currentNode = npc.startingNode;
            }
        }

        dialoguePanel.SetActive(true);
        DisplayNode(currentNode);
    }


    public void DisplayNode(DialogueNode node)
    {
        currentNode = node;

        bool isFallback = string.IsNullOrEmpty(node.speakerName) || node.speakerName == "Narration";
        speakerText.text = isFallback ? "" : node.speakerName;

        if (portraitImage != null)
        {
            if (isFallback)
                portraitImage.enabled = false;
            else
            {
                portraitImage.sprite = node.portrait;
                portraitImage.enabled = node.portrait != null;
            }
        }

        //Toggle portrait background
        if (portraitBackground != null)
        {
            portraitBackground.SetActive(!isFallback);
        }

        if (choicePanelBackground != null)
        {
            bool hasChoices = node.choices != null && node.choices.Count > 0;
            choicePanelBackground.SetActive(hasChoices);
        }

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(node.dialogueText));

        currentChoices = new List<DialogueChoice>(node.choices);
        selectedIndex = 0;
        scrollOffset = 0;

        isWaitingForContinue = false;

        if (waitCoroutine != null)
        {
            StopCoroutine(waitCoroutine);
            waitCoroutine = null;
        }
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
        Debug.Log("Waiting for Space to continue from: " + currentNode.name);
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        yield return new WaitUntil(() => Input.GetKeyUp(KeyCode.Space));

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

        // Handle Simple Dialogue Mode
        if (simpleDialogueLines != null)
        {
            if (Input.GetKeyDown(KeyCode.Space) && typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                
                dialogueText.text = simpleDialogueLines[simpleDialogueIndex];
                typingCoroutine = null;

                if (typingAudioSource != null && typingAudioSource.isPlaying)
                    typingAudioSource.Stop();

                if (waitCoroutine != null)
                    StopCoroutine(waitCoroutine);
                waitCoroutine = StartCoroutine(WaitForSimpleContinue());

                return;
            }

            return; // Prevent the rest of this method (which relies on currentNode)
        }


        // Handle Node-Based Dialogue (standard NPC quests)
        if (Input.GetKeyDown(KeyCode.Space) && typingCoroutine != null)
        {
            
            StopCoroutine(typingCoroutine);

            dialogueText.text = currentNode.dialogueText;
            typingCoroutine = null;
            if (typingAudioSource != null && typingAudioSource.isPlaying)
                    typingAudioSource.Stop();

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
            {
                switch (choice.consequenceID)
                {
                    case "ANGRY_GUARD":
                        DisturbableNPC d = currentNPC?.GetComponent<DisturbableNPC>();
                        d?.TriggerAttack();
                        break;

                    case "TAKE_VERDANT_EMBER":
                        MemoryFlags.Set("TAKE_VERDANT_EMBER");
                        StartCoroutine(PlayEmberClaimSequence());
                        QuestManager.Instance?.IncrementObjective("COLLECT_EMBER", "EMBER_VERDANT", 1);;
                        break;

                    case "LEAVE_VERDANT_EMBER":
                        EmberManager.Instance.SetEmber(null, EmberData.WorldShiftType.Overgrown);
                        QuestManager.Instance?.IncrementObjective("COLLECT_EMBER", "EMBER_VERDANT", 1);

                        GridOvergrowthManager overgrowth = FindObjectOfType<GridOvergrowthManager>();
                        if (overgrowth != null)
                            overgrowth.ActivateOvergrowth();

                        break;

                    case "TAKE_DOMINION_EMBER":
                        MemoryFlags.Set("TAKE_DOMINION_EMBER");
                        StartCoroutine(PlayDominionEmberClaimSequence());
                        QuestManager.Instance?.IncrementObjective("COLLECT_EMBER", "EMBER_DOMINION", 1);
                        break;

                    case "LEAVE_DOMINION_EMBER":
                        MemoryFlags.Set("LEAVE_DOMINION_EMBER");
                        EmberManager.Instance.SetEmber(null, EmberData.WorldShiftType.Reforged);
                        QuestManager.Instance?.IncrementObjective("COLLECT_EMBER", "EMBER_DOMINION", 1);
                        break;
                    case "TAKE_WRATH_EMBER":
                        MemoryFlags.Set("TAKE_WRATH_EMBER");
                        StartCoroutine(PlayWrathEmberClaimSequence());
                        QuestManager.Instance?.IncrementObjective("COLLECT_EMBER", "EMBER_WRATH", 1);
                        break;

                    case "LEAVE_WRATH_EMBER":
                        MemoryFlags.Set("LEAVE_DOMINION_EMBER");
                        QuestManager.Instance?.IncrementObjective("COLLECT_EMBER", "EMBER_WRATH", 1);
                        EmberManager.Instance.SetEmber(null, EmberData.WorldShiftType.Extinguished);
                         CameraShaker shaker = GameObject.Find("CmCam")?.GetComponent<CameraShaker>();
                        if (shaker != null)
                            shaker.Shake(7f, 0.5f);
                        else
                            Debug.LogWarning("CameraShaker not found on CmCam.");
                            WrathEnemySpawner.Instance?.TriggerSpawns();
                        break;
                    case "LOAD_FINAL_SCENE":
                            if (EndingTracker.Instance != null)
                            {
                                string ending = EndingTracker.Instance.GetEndingID();
                                Debug.Log("🧮 Current Ending Score: " + ending);
                                Debug.Log("Flags: " +
                                    $"V:{MemoryFlags.Get("TAKE_VERDANT_EMBER")}/{MemoryFlags.Get("LEAVE_VERDANT_EMBER")}, " +
                                    $"D:{MemoryFlags.Get("TAKE_DOMINION_EMBER")}/{MemoryFlags.Get("LEAVE_DOMINION_EMBER")}, " +
                                    $"W:{MemoryFlags.Get("TAKE_WRATH_EMBER")}/{MemoryFlags.Get("LEAVE_WRATH_EMBER")}");

                                switch (ending)
                                {
                                    case "GOOD_ENDING":
                                        SceneManager.LoadScene("FinalScene");
                                        break;
                                    case "NEUTRAL_ENDING":
                                        SceneManager.LoadScene("FinalScene_Neutral");
                                        break;
                                    case "BAD_ENDING":
                                        SceneManager.LoadScene("FinalScene_Bad");
                                        break;
                                    default:
                                        Debug.LogWarning("⚠️ No valid ending detected. Defaulting to FinalScene.");
                                        SceneManager.LoadScene("FinalScene");
                                        break;
                                }
                            }
                            else
                            {
                                Debug.LogError("❌ EndingTracker.Instance is null. Cannot load ending scene.");
                                SceneManager.LoadScene("FinalScene"); // Optional fallback
                            }
                            break;

                    default:
                        if (choice.consequenceID.StartsWith("GIVE_"))
                        {
                            string questID = choice.consequenceID.Substring(5);
                            GiveQuestFromNPC(questID);
                        }
                        else
                        {
                            MemoryFlags.Set(choice.consequenceID);
                        }
                        break;
                }
            }

            if (choice.nextNode != null)
                DisplayNode(choice.nextNode);
            else
                EndDialogue();
        }
        // Press Escape to exit dialogue during a choice node
        if (currentChoices != null && currentChoices.Count > 0 && Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Dialogue cancelled by player (Escape pressed).");
            EndDialogue();
            return;
        }
    }

    void EndDialogue()
    {
        if (waitCoroutine != null)
        {
            StopCoroutine(waitCoroutine);
            waitCoroutine = null;
        }
        if (typingAudioSource != null)
            typingAudioSource.Stop();

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        dialoguePanel.SetActive(false);
        ClearChoices();
        GameStateController.Instance?.SetDialogueState(false);

        // No currentChoices.Clear() — avoids modifying shared ScriptableObject
    }

    IEnumerator TypeText(string fullText)
    {
        dialogueText.text = "";

        // Start typing sound
        if (typingAudioSource != null && !typingAudioSource.isPlaying)
            typingAudioSource.Play();

        foreach (char c in fullText)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        // Stop sound after line is typed
        if (typingAudioSource != null && typingAudioSource.isPlaying)
            typingAudioSource.Stop();

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

    void GiveQuestFromNPC(string questID)
    {
        if (currentNPC == null || currentNPC.assignedQuest == null) return;

        if (currentNPC.assignedQuest != null && currentNPC.assignedQuest.questID == questID)
        {
            QuestData runtimeQuest = QuestManager.Instance.CreateRuntimeCopy(currentNPC.assignedQuest);
            runtimeQuest.questGiven = true;
            runtimeQuest.isActive = true;
            QuestManager.Instance.GiveQuest(runtimeQuest);
            currentNPC.activeRuntimeQuest = runtimeQuest;

            currentNPC.GetComponent<TalkPromptController>()?.UpdateQuestMarker();

            QuestLogUI questLog = FindObjectOfType<QuestLogUI>();
            if (questLog != null && questLog.questLogPage.activeSelf)
            {
                questLog.UpdateQuestList();
            }
        }
    }

    private DialogueNode CreateFallbackNode(string speaker, string message)
    {
        DialogueNode fallback = ScriptableObject.CreateInstance<DialogueNode>();
        fallback.speakerName = ""; // display as narration
        fallback.dialogueText = message;
        fallback.choices = new List<DialogueChoice>();

        if (currentNPC != null && currentNPC.startingNode != null)
        {
            fallback.portrait = currentNPC.startingNode.portrait;
        }

        return fallback;
    }

    // ========== SIMPLE DIALOGUE SUPPORT ==========
    private string[] simpleDialogueLines;
    private int simpleDialogueIndex = 0;
    private System.Action onSimpleDialogueComplete;

    public void StartSimpleDialogue(string[] lines, string speakerName, Sprite portrait = null, System.Action onComplete = null)
    {
        if (GameStateController.IsCutscenePlaying && dialoguePanel.activeSelf)
        {
            Debug.Log("Dialogue already running during cutscene.");
            return;
        }

        if (GameStateController.IsDialogueActive)
        {
            Debug.Log("Simple dialogue already active.");
            return;
        }

        GameStateController.Instance?.SetDialogueState(true);

        simpleDialogueLines = lines;
        simpleDialogueIndex = 0;
        onSimpleDialogueComplete = onComplete;

        speakerText.text = speakerName;

        if (portraitImage != null)
        {
            portraitImage.sprite = portrait;
            portraitImage.enabled = portrait != null;
        }

        portraitBackground?.SetActive(portrait != null);
        choicePanelBackground?.SetActive(false);
        dialoguePanel.SetActive(true);

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeSimpleLine(simpleDialogueLines[simpleDialogueIndex]));
    }
    IEnumerator TypeSimpleLine(string line)
    {
        dialogueText.text = "";

        if (typingAudioSource != null && !typingAudioSource.isPlaying)
            typingAudioSource.Play();

        foreach (char c in line)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        if (typingAudioSource != null && typingAudioSource.isPlaying)
            typingAudioSource.Stop();

        typingCoroutine = null;
        isWaitingForContinue = true;

        if (waitCoroutine != null)
            StopCoroutine(waitCoroutine);
        waitCoroutine = StartCoroutine(WaitForSimpleContinue());
    }


    IEnumerator WaitForSimpleContinue()
    {
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        yield return new WaitUntil(() => Input.GetKeyUp(KeyCode.Space));

        isWaitingForContinue = false;
        simpleDialogueIndex++;

        if (simpleDialogueIndex < simpleDialogueLines.Length)
        {
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            typingCoroutine = StartCoroutine(TypeSimpleLine(simpleDialogueLines[simpleDialogueIndex]));
        }
        else
        {
            EndSimpleDialogue();
        }

        waitCoroutine = null;
    }

    void EndSimpleDialogue()
    {
        dialoguePanel.SetActive(false);
        dialogueText.text = "";
        simpleDialogueLines = null;
        onSimpleDialogueComplete?.Invoke();
        GameStateController.Instance?.SetDialogueState(false);
    }

    public void ForceEndDialogue()
    {
        if (typingAudioSource != null)
             typingAudioSource.Stop();
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (waitCoroutine != null)
        {
            StopCoroutine(waitCoroutine);
            waitCoroutine = null;
        }

        ClearChoices();
        dialogueText.text = "";
        dialoguePanel.SetActive(false);

        GameStateController.Instance?.SetDialogueState(false);
    }

    private IEnumerator PlayEmberClaimSequence()
    {
        Debug.Log("🔥 Playing Ember Claim Animation...");

        // 1. Trigger camera shake — BEFORE the ember floats
        CameraShaker shaker = GameObject.Find("CmCam")?.GetComponent<CameraShaker>();
        if (shaker != null)
        {
            shaker.Shake(7f, 0.5f);
        }
        else
        {
            Debug.LogWarning("CameraShaker not found on CmCam.");
        }

        // 2. Trigger ember float + fade
        GameObject ember = GameObject.FindWithTag("VerdantEmber");
        if (ember != null)
        {
            EmberFloatEffect fx = ember.GetComponent<EmberFloatEffect>();
            if (fx != null)
            {
                fx.PlayEffect();
            }
            else
            {
                Debug.LogWarning("EmberFloatEffect not found on Ember.");
            }
        }
        else
        {
            Debug.LogWarning("VerdantEmber not found in scene.");
        }

        // 3. Wait for animation to finish
        yield return new WaitForSeconds(1.7f);

        // 4. Apply Ember ability + world shift
        EmberManager.Instance.SetEmber(GameStateController.Instance.verdantEmberData, EmberData.WorldShiftType.Seeded);
        Debug.Log("Ember claimed. Bloomstep granted.");
    }
    private IEnumerator PlayDominionEmberClaimSequence()
    {
        Debug.Log("🔥 Playing Dominion Ember Claim Animation...");

        // 1. Camera shake
        CameraShaker shaker = GameObject.Find("CmCam")?.GetComponent<CameraShaker>();
        if (shaker != null)
        {
            shaker.Shake(7f, 0.5f);
        }
        else
        {
            Debug.LogWarning("CameraShaker not found on CmCam.");
        }

        // 2. Find Dominion Ember in scene
        GameObject ember = GameObject.FindWithTag("DominionEmber");
        if (ember != null)
        {
            EmberFloatEffect fx = ember.GetComponent<EmberFloatEffect>();
            if (fx != null)
            {
                fx.PlayEffect();
            }
            else
            {
                Debug.LogWarning("EmberFloatEffect not found on Dominion Ember.");
            }
        }
        else
        {
            Debug.LogWarning("DominionEmber not found in scene.");
        }

        // 3. Wait for visual effect
        yield return new WaitForSeconds(1.7f);

        // 4. Grant command pulse
        EmberManager.Instance.SetEmber(GameStateController.Instance.dominionEmberData, EmberData.WorldShiftType.Default);
        Debug.Log(" Dominion Ember claimed. Command Pulse granted.");
    }

    private IEnumerator DelayedQuestUIUpdate()
    {
        yield return new WaitForEndOfFrame();
        QuestLogUI.Instance?.UpdateQuestList();
    }
    private IEnumerator PlayWrathEmberClaimSequence()
    {
        Debug.Log("🔥 Playing Wrath Ember Claim Animation...");

        CameraShaker shaker = GameObject.Find("CmCam")?.GetComponent<CameraShaker>();
        if (shaker != null)
            shaker.Shake(7f, 0.5f);
        else
            Debug.LogWarning("CameraShaker not found on CmCam.");

        GameObject ember = GameObject.FindWithTag("WrathEmber");
        if (ember != null)
        {
            EmberFloatEffect fx = ember.GetComponent<EmberFloatEffect>();
            if (fx != null)
                fx.PlayEffect();
        }
        else
        {
            Debug.LogWarning("WrathEmber not found in scene.");
        }

        yield return new WaitForSeconds(1.7f);

        EmberManager.Instance.SetEmber(GameStateController.Instance.wrathEmberData, EmberData.WorldShiftType.Default);
        Debug.Log(" Wrath Ember claimed. FlameGuard granted.");
    }

}
