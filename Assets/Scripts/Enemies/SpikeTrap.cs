using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    public float cooldownTime = 2f; // Time between spikes
    private float timer = 0f;
    private bool isReady = true;
    private bool isActive = false;

    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (!isReady)
        {
            timer += Time.deltaTime;
            if (timer >= cooldownTime)
            {
                isReady = true;
                timer = 0f;
            }
        }

        if (isReady)
        {
            anim.SetTrigger("Spike");
            isReady = false;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (isActive && other.CompareTag("Player"))
        {
            PlayerHealth player = other.GetComponent<PlayerHealth>();
            if (player != null)
            {
                player.TakeDamage(1);
                isActive = false; // Prevent multi-hit
            }
        }
    }

    // Animation Events
    public void ActivateDamageWindow() => isActive = true;
    public void DeactivateDamageWindow() => isActive = false;
}
