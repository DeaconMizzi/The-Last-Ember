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

    // Reference to the actual quest
    public QuestData quest = new QuestData();

    // Controls post-quest behavior
    public bool canTalkAfterQuest = false;
    public DialogueNode postQuestNode;
}
