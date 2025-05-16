using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateController : MonoBehaviour
{
    public static GameStateController Instance { get; private set; }
    public GameObject player;
    public GameObject healthUI;
    public GameObject[] enemies;
    public EmberData verdantEmberData;
    public EmberData wrathEmberData;
    public EmberData dominionEmberData;


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }
    public void FreezeGame()
    {
        Debug.Log("FreezeGame called!");

        if (player == null)
        {
            Debug.LogWarning("Player not assigned!");
            return;
        }

        var move = player.GetComponent<PlayerMovement>();
        var combat = player.GetComponent<PlayerCombat>();

        if (move != null)
        {
            move.enabled = false;
            Debug.Log("Disabled PlayerMovement");
        }
        else
        {
            Debug.LogWarning("PlayerMovement NOT FOUND on player!");
        }

        if (combat != null)
        {
            combat.enabled = false;
            Debug.Log("Disabled PlayerCombat");
        }
        else
        {
            Debug.LogWarning("PlayerCombat NOT FOUND on player!");
        }

        if (healthUI != null)
        {
            healthUI.SetActive(false);
            Debug.Log("Health UI HIDDEN");
        }
        else
        {
            Debug.LogWarning("Health UI not assigned.");
        }

        foreach (var e in enemies)
        {
            if (e == null) continue;

            var ai = e.GetComponent<OrcAI>();
            if (ai != null)
            {
                ai.enabled = false;
                Debug.Log("Disabled OrcAI on: " + e.name);
            }
            else
            {
                Debug.LogWarning("OrcAI not found on " + e.name);
            }
        }
    }

    public static bool IsCutscenePlaying { get; private set; }

    public void SetCutsceneState(bool state)
    {
        IsCutscenePlaying = state;
    }

    public void UnfreezeGame()
    {
        Debug.Log("UnfreezeGame called!");

        var move = player.GetComponent<PlayerMovement>();
        var combat = player.GetComponent<PlayerCombat>();

        if (move != null) move.enabled = true;
        if (combat != null) combat.enabled = true;

        if (healthUI != null) healthUI.SetActive(true);

        foreach (var e in enemies)
        {
            if (e == null) continue;

            var ai = e.GetComponent<OrcAI>();
            if (ai != null) ai.enabled = true;
        }
    }

    public static bool IsDialogueActive { get; private set; }

    public void SetDialogueState(bool state)
    {
        IsDialogueActive = state;
    }
    void Start()
    {
        QuestManager.Instance.ResetAllQuestStates();
        FreezeGame(); // if you're already using that
    }
}
