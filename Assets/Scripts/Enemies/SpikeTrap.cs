using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    public float cooldownTime = 2f; // Time after retracting

    [Header("Random Start Delay")]
    public float minStartDelay = 0f;
    public float maxStartDelay = 1.5f;

    private bool isReady = true;
    private bool isActive = false;

    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
        StartCoroutine(DelayedStart());
    }

    IEnumerator DelayedStart()
    {
        float delay = Random.Range(minStartDelay, maxStartDelay);
        yield return new WaitForSeconds(delay);
        StartCoroutine(SpikeCycle());
    }

    IEnumerator SpikeCycle()
    {
        while (true)
        {
            if (isReady)
            {
                anim.SetTrigger("Spike"); // Triggers Spikes_Up
                isReady = false;

                // Wait for the full cycle to play (Up + Down)
                float fullCycleTime = GetAnimationLength("Spikes_Up") + GetAnimationLength("Spikes_Down");
                yield return new WaitForSeconds(fullCycleTime + cooldownTime);

                isReady = true;
            }
            yield return null;
        }
    }

    float GetAnimationLength(string clipName)
    {
        foreach (AnimationClip clip in anim.runtimeAnimatorController.animationClips)
        {
            if (clip.name == clipName)
                return clip.length;
        }
        return 1f; // fallback if not found
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (isActive && other.CompareTag("Player"))
        {
            PlayerHealth player = other.GetComponent<PlayerHealth>();
            if (player != null)
            {
                player.TakeDamage(1);
                isActive = false; // Prevent repeated damage during active frame
            }
        }
    }

    // Animation Events (called via spike animation)
    public void ActivateDamageWindow()
    {
        isActive = true;

        // Immediately check for overlapping player
        Collider2D[] hits = Physics2D.OverlapBoxAll(transform.position, GetComponent<BoxCollider2D>().bounds.size, 0f);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerHealth player = hit.GetComponent<PlayerHealth>();
                if (player != null)
                {
                    player.TakeDamage(1);
                    isActive = false;
                }
            }
        }
    }

    public void DeactivateDamageWindow() => isActive = false;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, GetComponent<BoxCollider2D>().bounds.size);
    }
}
