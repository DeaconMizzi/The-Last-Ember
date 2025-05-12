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
            if (quest == null || string.IsNullOrEmpty(quest.questID))
            {
                Debug.LogWarning("Invalid quest found.");
                continue;
            }

            Debug.Log("QuestLogUI: Adding quest: " + quest.questID);

            string desc = string.IsNullOrEmpty(quest.description) ? "No description." : quest.description;
            string status = quest.isComplete ? "<color=green>✔ Completed</color>" : "";
            string typeLabel = quest.questType == QuestType.Main ? "<color=yellow>[Main]</color>" : "<color=blue>[Side]</color>";

            questText.text += $"{typeLabel} <b>{quest.questName}</b> {status}\n<size=85%>{desc}</size>\n";

            foreach (var obj in quest.objectives)
            {
                string objDesc = "";

                switch (obj.type)
                {
                    case ObjectiveType.KillTarget:
                        objDesc = $"Kill {obj.targetID}: {obj.currentCount}/{obj.requiredCount}";
                        break;
                    case ObjectiveType.CollectItem:
                        objDesc = $"Collect {obj.targetID}: {obj.currentCount}/{obj.requiredCount}";
                        break;
                    case ObjectiveType.ReachArea:
                        objDesc = $"Reach {obj.targetID}: {(obj.IsComplete ? "✔" : "❌")}";
                        break;
                    case ObjectiveType.TalkToNPC:
                        objDesc = $"Talk to {obj.targetID}: {(obj.IsComplete ? "✔" : "❌")}";
                        break;
                }

                questText.text += $"<size=80%><i>{objDesc}</i></size>\n";
            }

            questText.text += "\n";
        }

        if (string.IsNullOrEmpty(questText.text))
        {
            questText.text = "<i>No active quests.</i>";
        }
    }
}
