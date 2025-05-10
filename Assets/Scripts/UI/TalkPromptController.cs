using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TalkPromptController : MonoBehaviour
{
    public GameObject promptUI;        // E key
    public GameObject questMarkerUI;   // ! icon
    private Transform player;
    private float displayDistance = 1.5f;
    private NPC npc;
    private float npcHeight = 1f; // Cached height from BoxCollider2D
    private Coroutine popRoutine_Quest;
    private Coroutine popRoutine_Prompt;

    private bool wasInRange = false;

    private Vector3 questMarkerBasePos;
    private Vector3 promptBasePos;

    public float bobbingSpeed = 2f;
    public float bobbingAmount = 0.05f;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        npc = GetComponent<NPC>();

        if (npc == null)
        {
            if (promptUI != null) promptUI.SetActive(false);
            if (questMarkerUI != null) questMarkerUI.SetActive(false);
            enabled = false;
            return;
        }

        if (promptUI != null)
            promptUI.SetActive(false); // hide E at start

        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null)
            npcHeight = col.size.y;

        PositionUIElements();
        UpdateQuestMarker();
    }

    void Update()
    {
        float dist = Vector2.Distance(player.position, transform.position);
        bool inRange = dist <= displayDistance;

        // Toggle E prompt with pop effect
        if (promptUI != null)
        {
            if (inRange && !wasInRange)
            {
                promptUI.SetActive(true);
                if (popRoutine_Prompt != null) StopCoroutine(popRoutine_Prompt);
                popRoutine_Prompt = StartCoroutine(PopUI(promptUI));
            }
            else if (!inRange && wasInRange)
            {
                promptUI.SetActive(false);
            }
        }

        wasInRange = inRange;

        // Update floating positions
        ShiftQuestMarker(inRange);
    }

    void LateUpdate()
    {
        float offset = Mathf.Sin(Time.time * bobbingSpeed) * bobbingAmount;

        if (questMarkerUI != null && questMarkerUI.activeSelf)
            questMarkerUI.transform.localPosition = questMarkerBasePos + new Vector3(0f, offset, 0f);

        if (promptUI != null && promptUI.activeSelf)
            promptUI.transform.localPosition = promptBasePos + new Vector3(0f, offset, 0f);
    }

    public void UpdateQuestMarker()
    {
        if (questMarkerUI == null || npc == null)
        {
            if (questMarkerUI != null)
                questMarkerUI.SetActive(false);
            return;
        }

        bool shouldShow = npc.quest != null && !npc.quest.questGiven;

        if (shouldShow && !questMarkerUI.activeSelf)
        {
            questMarkerUI.SetActive(true);

            if (popRoutine_Quest != null) StopCoroutine(popRoutine_Quest);
            popRoutine_Quest = StartCoroutine(PopUI(questMarkerUI));
        }
        else if (!shouldShow && questMarkerUI.activeSelf)
        {
            questMarkerUI.SetActive(false);
        }
    }

    private IEnumerator PopUI(GameObject target)
    {
        Vector3 originalScale = target.transform.localScale;
        target.transform.localScale = Vector3.zero;

        float duration = 0.25f;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float scale = Mathf.SmoothStep(0f, 1f, t / duration);
            target.transform.localScale = originalScale * scale;
            yield return null;
        }

        target.transform.localScale = originalScale;
    }

    private void PositionUIElements()
    {
        if (questMarkerUI != null)
        {
            questMarkerBasePos = new Vector3(0f, npcHeight, 0f);
            questMarkerUI.transform.localPosition = questMarkerBasePos;
        }

        if (promptUI != null)
        {
            promptBasePos = new Vector3(0.25f, npcHeight, 0f);
            promptUI.transform.localPosition = promptBasePos;
        }
    }

    private void ShiftQuestMarker(bool playerInRange)
    {
        if (questMarkerUI == null || promptUI == null) return;

        bool hasQuestMarker = questMarkerUI.activeSelf;

        if (playerInRange)
        {
            if (hasQuestMarker)
            {
                questMarkerBasePos = new Vector3(-0.50f, npcHeight, 0f);
                promptBasePos = new Vector3(0.50f, npcHeight, 0f); // "E" on the right
            }
            else
            {
                promptBasePos = new Vector3(0f, npcHeight, 0f); // "E" centered
            }
        }
        else
        {
            questMarkerBasePos = new Vector3(0f, npcHeight, 0f); // recenter "!"
        }
    }
}
