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
            var quest = activeQuests[questID];
            quest.isComplete = true;
            quest.hasCompletedQuest = true;

            // Update the display mark
            if (quest.description.Contains("-"))
                quest.description = quest.description.Replace("-", "✓");

            Debug.Log("Quest completed: " + questID);

            QuestLogUI.Instance?.UpdateQuestList();
            FindObjectOfType<QuestPopupUI>()?.ShowPopup($"{quest.questName} completed!");
        }
    }

    public void RegisterEnemyKill(EnemyType killedType)
    {
        foreach (var quest in activeQuests.Values)
        {
            if (quest.isComplete) continue;

            foreach (var obj in quest.objectives)
            {
                if (obj.type == ObjectiveType.KillTarget && obj.targetID == killedType.ToString() && !obj.IsComplete)
                {
                    obj.currentCount++;
                    Debug.Log($"Quest {quest.questID} - {obj.targetID}: {obj.currentCount}/{obj.requiredCount}");

                    if (quest.AllObjectivesComplete)
                    {
                        quest.isComplete = true;
                        Debug.Log($"Quest completed: {quest.questID}");
                    }

                    FindObjectOfType<QuestLogUI>()?.UpdateQuestList();
                    FindObjectOfType<QuestPopupUI>()?.ShowPopup($"{quest.questName}: {obj.currentCount}/{obj.requiredCount}");

                    break;
                }
            }
        }
    }

    public bool IsQuestComplete(string questID)
    {
        return activeQuests.ContainsKey(questID) && activeQuests[questID].isComplete;
    }

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

    public void ResetAllQuestStates()
    {
        foreach (var quest in Resources.LoadAll<QuestData>(""))
        {
            quest.isActive = false;
            quest.questGiven = false;
            quest.isComplete = false;
            quest.hasCompletedQuest = false;
            Debug.Log($"Reset quest: {quest.questID} (given: {quest.questGiven}, active: {quest.isActive})");


            foreach (var obj in quest.objectives)
            {

                obj.currentCount = 0;
            }
        }

        Debug.Log("All quest states reset.");
    }

    public QuestData CreateRuntimeCopy(QuestData original)
    {
        QuestData clone = ScriptableObject.Instantiate(original);

        // Reset runtime fields
        clone.isActive = false;
        clone.isComplete = false;
        clone.questGiven = false;
        clone.hasCompletedQuest = false;

        foreach (var obj in clone.objectives)
        {
            obj.currentCount = 0;
        }

        return clone;
    }
    
    public void IncrementObjective(string questID, string targetID, int amount)
    {
        if (!activeQuests.ContainsKey(questID))
        {
            Debug.LogWarning($"Quest {questID} is not active.");
            return;
        }

        QuestData quest = activeQuests[questID];
        foreach (var obj in quest.objectives)
        {
            if (obj.targetID == targetID)
            {
                obj.currentCount += amount;

                // Clamp to avoid overflow
                if (obj.currentCount > obj.requiredCount)
                    obj.currentCount = obj.requiredCount;

                Debug.Log($"Updated {questID} objective {targetID} to {obj.currentCount}/{obj.requiredCount}");
                break;
            }
        }

        // Check if all objectives are complete
        bool allComplete = true;
        foreach (var obj in quest.objectives)
        {
            if (obj.currentCount < obj.requiredCount)
            {
                allComplete = false;
                break;
            }
        }

        if (allComplete)
        {
            quest.isComplete = true;
            quest.hasCompletedQuest = true;
            Debug.Log($"Quest {questID} completed!");
        }

        QuestLogUI.Instance?.UpdateQuestList();
    }
}