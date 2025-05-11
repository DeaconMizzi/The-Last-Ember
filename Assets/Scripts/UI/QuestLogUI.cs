using UnityEngine;
using TMPro;

public class QuestLogUI : MonoBehaviour
{
    public GameObject questLogPage;
    public TMP_Text questText;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            bool isActive = !questLogPage.activeSelf;
            questLogPage.SetActive(isActive);

            if (isActive)
                UpdateQuestList();
        }
    }

    public void UpdateQuestList()
    {
        questText.text = "";

        var allQuests = QuestManager.Instance.GetAllQuests();
        Debug.Log("QuestLogUI: Found " + allQuests.Count + " quests.");

        foreach (var quest in allQuests)
        {
            if (quest == null)
            {
                Debug.LogWarning("Quest is null!");
                continue;
            }

            if (string.IsNullOrEmpty(quest.questID))
            {
                Debug.LogWarning("Quest has empty ID.");
                continue;
            }

            Debug.Log("QuestLogUI: Adding quest: " + quest.questID);

            string desc = string.IsNullOrEmpty(quest.description) ? "No description." : quest.description;
            string status = quest.isComplete ? "<color=green>✔ Completed</color>" : "";
            string progress = "";

            if (quest.objectiveType == ObjectiveType.Kill && !quest.isComplete)
            {
                progress = $"<i>{quest.currentCount} / {quest.targetCount} slain</i>";
            }

            string typeLabel = quest.questType == QuestType.Main ? "<color=yellow>[Main]</color>" : "<color=cyan>[Side]</color>";

            questText.text += $"{typeLabel} <b>{quest.questName}</b> {status}\n<size=85%>{desc}</size>";

            if (!string.IsNullOrEmpty(progress))
            {
                questText.text += $"\n<size=80%>{progress}</size>";
            }

            questText.text += "\n\n";
        }

        if (string.IsNullOrEmpty(questText.text))
        {
            questText.text = "<i>No active quests.</i>";
        }
    }
}
