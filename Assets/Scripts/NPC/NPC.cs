using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    public string speakerName;
    [TextArea] public string line;
    public Sprite portrait;
}

public class NPC : MonoBehaviour
{
    public string npcName;
    public DialogueNode startingNode;
    public DialogueNode questAcceptedNode;
    public DialogueNode questCompleteNode;

    [Header("One-Time Conditional Dialogue")]
    public string memoryFlagCondition;         // Flag that enables alternate line
    public string conditionalUsedFlagID;       // Flag that prevents it repeating
    public DialogueNode conditionalStartingNode;

    [Header("Quest")]
    public QuestData assignedQuest;
    [HideInInspector] public QuestData activeRuntimeQuest;
    public bool canTalkAfterQuest = true;

    void Start()
    {
        // Only set this if nothing else has initialized a quest
        if (assignedQuest != null && activeRuntimeQuest == null)
        {
            activeRuntimeQuest = assignedQuest;
        }
    }
}
