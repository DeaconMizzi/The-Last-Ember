using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class EmberActivator : MonoBehaviour
{
    [Header("Assign all components that handle interaction (e.g., NPC, TalkPromptController)")]
    public MonoBehaviour[] interactionComponents;

    [Header("Optional: UI prompt to hide/show (e.g. E icon)")]
    public GameObject promptUI;

    private Collider2D emberCollider;

    [Header("Unlock Conditions")]
    public string memoryFlag = "BRANHALM_DEFEATED";
    public GameObject enemyParent;
    public bool useEnemyClearCondition = false;

    private void Awake()
    {
        emberCollider = GetComponent<Collider2D>();
        if (emberCollider != null)
        {
            emberCollider.enabled = false;
            Debug.Log("[EmberActivator] ❌ Collider disabled at start");
        }

        foreach (var comp in interactionComponents)
        {
            if (comp != null)
            {
                comp.enabled = false;
                Debug.Log($"[EmberActivator] ❌ Disabled on Awake → {comp.GetType().Name}");
            }
        }

        if (promptUI != null)
        {
            promptUI.SetActive(false);
            Debug.Log("[EmberActivator] ❌ Prompt UI hidden at start");
        }
    }

    private void Update()
    {
        bool shouldActivate = false;

        if (!string.IsNullOrEmpty(memoryFlag) && MemoryFlags.Get(memoryFlag))
        {
            shouldActivate = true;
        }
        else if (useEnemyClearCondition && enemyParent != null)
        {
            shouldActivate = AreAllEnemiesDefeated();
        }

        if (shouldActivate)
        {
            if (emberCollider != null && !emberCollider.enabled)
            {
                emberCollider.enabled = true;
                Debug.Log("[EmberActivator] ✅ Collider enabled");
            }

            foreach (var comp in interactionComponents)
            {
                if (comp != null && !comp.enabled)
                {
                    comp.enabled = true;
                    Debug.Log($"[EmberActivator] ✅ Enabled {comp.GetType().Name}");
                }
            }

            if (promptUI != null && !promptUI.activeSelf)
            {
                promptUI.SetActive(true);
                Debug.Log("[EmberActivator] ✅ Prompt UI shown");
            }

            // Optional: disable further checks
            enabled = false;
        }
    }
    private bool AreAllEnemiesDefeated()
    {
        foreach (Transform child in enemyParent.transform)
        {
            if (child.gameObject.activeInHierarchy)
            {
                return false;
            }
        }
        return true;
    }
}
