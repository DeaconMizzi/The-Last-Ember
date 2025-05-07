using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    private Dictionary<string, QuestData> activeQuests = new Dictionary<string, QuestData>();

    private void Awake()
    {
        Instance = this;
    }

    public void GiveQuest(QuestData quest)
    {
        if (!activeQuests.ContainsKey(quest.questID))
        {
            activeQuests.Add(quest.questID, quest);
            Debug.Log("Quest started: " + quest.description);
        }
    }

    public void CompleteQuest(string questID)
    {
        if (activeQuests.ContainsKey(questID))
        {
            activeQuests[questID].isComplete = true;
            Debug.Log("Quest completed: " + questID);
        }
    }

    public bool IsQuestComplete(string questID)
    {
        return activeQuests.ContainsKey(questID) && activeQuests[questID].isComplete;
    }
}