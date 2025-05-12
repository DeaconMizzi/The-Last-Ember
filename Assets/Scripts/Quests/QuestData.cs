using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum QuestType
{
    Main,
    Side
}

public enum ObjectiveType
{
    None,
    ReachArea,
    KillTarget,
    CollectItem,
    TalkToNPC
}

[System.Serializable]
public class QuestObjective
{
    public ObjectiveType type;
    public string targetID; // Can be enemy name, area name, item ID, or NPC ID
    public int requiredCount = 1;
    public int currentCount = 0;

    public bool IsComplete => currentCount >= requiredCount;
}

[CreateAssetMenu(fileName = "NewQuest", menuName = "Quest/QuestData")]
public class QuestData : ScriptableObject
{
    public string questID;
    public string questName;

    [TextArea]
    public string description;

    public QuestType questType = QuestType.Side;

    public List<QuestObjective> objectives = new List<QuestObjective>();

    public bool isActive = false;
    public bool isComplete = false;
    public bool questGiven = false;
    public bool hasCompletedQuest = false;

    public bool AllObjectivesComplete => objectives.TrueForAll(obj => obj.IsComplete);
}
