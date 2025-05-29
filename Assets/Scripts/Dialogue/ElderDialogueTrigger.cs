using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class ElderDialogueTrigger : MonoBehaviour
{
    public GameStateController gameState;
    public PlayableDirector director;
    public string[] elderLines;
    public Sprite elderPortrait;
    public string questIDToGive = "COLLECT_EMBER";
    public bool hideSpeakerName = false; // ✅ New toggle to optionally hide speaker name
    public Animator fadeAnimator; // ✅ Optional fade animator
    public string fadeOutTriggerName = "FadeOut"; // ✅ Trigger or state name to play
    public GameObject fadePanelObject; // ✅ Panel or canvas to activate before fading

    public bool goToMenuAfterDialogue = false; // ✅ NEW: Optional toggle for menu transition

    private bool triggered = false;

    public void BeginDialogueSequence()
    {
        if (triggered) return;
        triggered = true;

        if (director != null)
            director.Pause();

        if (gameState != null)
            gameState.FreezeGame();

        string speaker = hideSpeakerName ? "" : "Elder";
        DialogueManager.Instance.StartSimpleDialogue(elderLines, speaker, elderPortrait, OnDialogueComplete);
    }

    void OnDialogueComplete()
    {
        if (TryGetComponent<NPC>(out NPC npc) && npc.assignedQuest != null && npc.assignedQuest.questID == questIDToGive)
        {
            npc.assignedQuest.questGiven = true;
            npc.assignedQuest.isActive = true;
            QuestManager.Instance.GiveQuest(npc.assignedQuest);

            QuestLogUI.Instance?.UpdateQuestList();
            npc.GetComponent<TalkPromptController>()?.UpdateQuestMarker();

            Debug.Log($"🧭 Gave quest '{npc.assignedQuest.questName}' from ElderDialogueTrigger.");
        }

        if (gameState != null)
            gameState.UnfreezeGame();

        if (director != null)
            director.Resume();

        if (fadePanelObject != null)
            fadePanelObject.SetActive(true);

        if (fadeAnimator != null)
            StartCoroutine(PlayFadeOutDelayed());

        if (goToMenuAfterDialogue)
        {
            SceneManager.LoadScene("Menu");
        }

        gameObject.SetActive(false);
    }

    IEnumerator PlayFadeOutDelayed()
    {
        yield return null; // wait one frame after enabling panel
        fadeAnimator.Play(fadeOutTriggerName);
        Debug.Log("🌙 FadeOut triggered after final dialogue (delayed).");
    }

    IEnumerator GoToMenuAfterFade()
    {
        yield return new WaitForSeconds(1.5f);
         Debug.Log("➡️ Loading Menu scene...");
        SceneManager.LoadScene("Menu");
    }
}
