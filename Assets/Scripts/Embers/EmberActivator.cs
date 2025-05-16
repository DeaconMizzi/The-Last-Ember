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
        if (MemoryFlags.Get("BRANHALM_DEFEATED"))
        {
            if (emberCollider != null && !emberCollider.enabled)
            {
                emberCollider.enabled = true;
                Debug.Log("[EmberActivator] ✅ Collider enabled after Branhalm defeat");
            }

            foreach (var comp in interactionComponents)
            {
                if (comp != null && !comp.enabled)
                {
                    comp.enabled = true;
                    Debug.Log($"[EmberActivator] ✅ Enabled {comp.GetType().Name} after Branhalm defeat");
                }
            }

            if (promptUI != null && !promptUI.activeSelf)
            {
                promptUI.SetActive(true);
                Debug.Log("[EmberActivator] ✅ Prompt UI shown after Branhalm defeat");
            }
        }
    }
}
