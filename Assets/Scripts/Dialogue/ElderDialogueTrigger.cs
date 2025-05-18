using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class ElderDialogueTrigger : MonoBehaviour
{
    public GameStateController gameState;
    public PlayableDirector director;
    public string[] elderLines;
    public Sprite elderPortrait;
    public string questIDToGive = "COLLECT_EMBER";

    private bool triggered = false;

    public void BeginDialogueSequence()
    {
        if (triggered) return;
        triggered = true;

        if (director != null)
            director.Pause();

        gameState.FreezeGame();
        DialogueManager.Instance.StartSimpleDialogue(elderLines, "Elder", elderPortrait, OnDialogueComplete);
    }

    void OnDialogueComplete()
    {
        // ✅ Directly reference this GameObject’s NPC component
        if (TryGetComponent<NPC>(out NPC npc) && npc.assignedQuest != null && npc.assignedQuest.questID == questIDToGive)
        {
            npc.assignedQuest.questGiven = true;
            npc.assignedQuest.isActive = true;
            QuestManager.Instance.GiveQuest(npc.assignedQuest);

            // ✅ Fix: Immediately update quest log so it's ready for first Tab press
            QuestLogUI.Instance?.UpdateQuestList();

            npc.GetComponent<TalkPromptController>()?.UpdateQuestMarker();

            Debug.Log($"🧭 Gave quest '{npc.assignedQuest.questName}' from ElderDialogueTrigger.");
        }
        else
        {
            Debug.LogWarning("❗ ElderDialogueTrigger could not give quest — NPC or assignedQuest missing/mismatched.");
        }

        gameState.UnfreezeGame();

        if (director != null)
            director.Resume();

        gameObject.SetActive(false);
    }
}
