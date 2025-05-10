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
            string status = quest.isComplete ? "<color=grey>(Completed)</color>" : "";

            questText.text += $"<b>{quest.questID}</b> {status}\n<size=85%>{desc}</size>\n\n";
        }

        if (string.IsNullOrEmpty(questText.text))
        {
            questText.text = "<i>No active quests.</i>";
        }
    }
}
