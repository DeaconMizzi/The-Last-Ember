using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;
    public event System.Action<string> OnQuestGiven;
    private Dictionary<string, QuestData> activeQuests = new Dictionary<string, QuestData>();

    private void Awake()
    {
        Instance = this;
        Debug.Log("QuestManager initialized");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            var popup = FindObjectOfType<QuestPopupUI>();
            if (popup != null)
            {
                Debug.Log("Found QuestPopupUI. Showing popup...");
                popup.ShowPopup("Test Quest: 2 / 5");
            }
            else
            {
                Debug.LogWarning("QuestPopupUI NOT found in the scene!");
            }
        }
    }
    public void GiveQuest(QuestData quest)
    {
        if (!activeQuests.ContainsKey(quest.questID))
        {
            activeQuests.Add(quest.questID, quest);
            Debug.Log("Quest started: " + quest.description);

            OnQuestGiven?.Invoke(quest.questID);
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

    public void RegisterEnemyKill(EnemyType killedType)
    {
        foreach (var quest in activeQuests.Values)
        {
            if (quest.objectiveType == ObjectiveType.Kill &&
                quest.targetEnemyType == killedType &&
                !quest.isComplete)
            {
                quest.currentCount++;

                if (quest.currentCount >= quest.targetCount)
                {
                    quest.isComplete = true;
                    Debug.Log($"Quest completed: {quest.questID}");
                }

                FindObjectOfType<QuestLogUI>()?.UpdateQuestList();

                string msg = $"{quest.questName}: {quest.currentCount} / {quest.targetCount}";
                FindObjectOfType<QuestPopupUI>()?.ShowPopup(msg);

                break;
            }
        }
    }
    public bool IsQuestComplete(string questID)
    {
        return activeQuests.ContainsKey(questID) && activeQuests[questID].isComplete;
    }

    // ✅ NEW: Return a list of all active quests
    public List<QuestData> GetAllQuests()
    {
        return new List<QuestData>(activeQuests.Values);
    }

    public void RemoveQuest(string questID)
    {
        if (activeQuests.ContainsKey(questID))
        {
            activeQuests.Remove(questID);
            Debug.Log("Quest removed: " + questID);
        }
    }
    }
