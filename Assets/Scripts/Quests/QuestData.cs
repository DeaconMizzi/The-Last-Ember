using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum QuestType
{
    Main,
    Side
}

[System.Serializable]
public enum ObjectiveType
{
    None,
    Kill
}

[System.Serializable]
public class QuestData
{
    public string questID;
    public string questName;
    [TextArea] public string description;

    public bool isComplete = false;
    public bool isActive = false;
    public bool questGiven = false;
    public bool hasCompletedQuest = false;

    public QuestType questType = QuestType.Side;
    public ObjectiveType objectiveType = ObjectiveType.None;
    public EnemyType targetEnemyType;
    public int targetCount;
    public int currentCount;
}
