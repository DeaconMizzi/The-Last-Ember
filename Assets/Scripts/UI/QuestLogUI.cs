using UnityEngine;
using TMPro;
using System.Collections;

public class QuestLogUI : MonoBehaviour
{
    public static QuestLogUI Instance { get; private set; }

    public GameObject questLogPage;
    public TMP_Text questText;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            bool isActive = !questLogPage.activeSelf;
            questLogPage.SetActive(isActive);

            if (isActive)
                StartCoroutine(DelayedQuestUpdate());
        }
    }

    private IEnumerator DelayedQuestUpdate()
    {
        yield return new WaitForEndOfFrame(); // Ensures QuestManager.Instance is ready
        if (QuestManager.Instance == null)
        {
            questText.text = "<i>Loading quests...</i>";
            yield break;
        }

        UpdateQuestList();
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
                        objDesc = $"Defeat {obj.targetID}: {obj.currentCount}/{obj.requiredCount}";
                        break;
                    case ObjectiveType.CollectItem:
                        objDesc = $"Collect {obj.targetID}: {obj.currentCount}/{obj.requiredCount}";
                        break;
                    case ObjectiveType.ReachArea:
                        string areaName = obj.targetID switch
                        {
                            "EMBER_VERDANT" => "the Verdant Spire",
                            "EMBER_DOMINION" => "Caldrith Keep",
                            "EMBER_WRATH" => "the Scorched Reach",
                            _ => obj.targetID
                        };
                        objDesc = $"Discover {areaName}: {(obj.IsComplete ? "X" : "-")}";
                        break;
                    case ObjectiveType.TalkToNPC:
                        objDesc = $"Speak with {obj.targetID}: {(obj.IsComplete ? "X" : "-")}";
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

    public void ShowQuestPopup(string message)
    {
        Debug.Log($"[Journal] {message}");
    }
}
