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
    public string questIDToGive = "firstember";

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
        if (QuestManager.Instance != null)
        {
            foreach (var npc in FindObjectsOfType<NPC>())
            {
                if (npc.assignedQuest != null && npc.assignedQuest.questID == questIDToGive)
                {
                    npc.assignedQuest.questGiven = true;
                    npc.assignedQuest.isActive = true;
                    QuestManager.Instance.GiveQuest(npc.assignedQuest);
                    npc.GetComponent<TalkPromptController>()?.UpdateQuestMarker();
                    break;
                }
            }
        }

        gameState.UnfreezeGame();

        if (director != null)
            director.Resume();

        gameObject.SetActive(false);
    }
}
