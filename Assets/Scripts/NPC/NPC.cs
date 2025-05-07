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

[System.Serializable]
public class QuestData
{
    public string questID;
    public string description;
    public bool isComplete = false;
}

public class NPC : MonoBehaviour
{
    public string npcName;
    public List<DialogueLine> dialogue;
    public QuestData quest;

    public bool questGiven = false;
}