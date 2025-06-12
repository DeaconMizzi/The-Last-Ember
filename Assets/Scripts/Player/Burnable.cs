using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Burnable : MonoBehaviour
{
    public float burnDamagePerTick = 0.2f;
    public float burnDuration = 5f;
    public float burnTickRate = 1f;
    public GameObject flameTrail;

    private bool isBurning = false;
    private Coroutine burnRoutine;

    public void ApplyBurn()
    {
        if (!isBurning)
        {
            burnRoutine = StartCoroutine(BurnCoroutine());
        }
    }

    private IEnumerator BurnCoroutine()
    {
        isBurning = true;
        Debug.Log("🔥 Burn started");

        if (flameTrail != null)
            flameTrail.SetActive(true);

        int ticks = 2;
        float interval = burnDuration / ticks;

        for (int i = 0; i < ticks; i++)
        {
            // Damage
            PlayerHealth health = GetComponent<PlayerHealth>();
            if (health != null)
                health.TakeDamage(Mathf.CeilToInt(burnDamagePerTick)); 

            // Hurt animation
            Animator anim = GetComponent<Animator>();
            if (anim != null)
                anim.SetTrigger("Hurt");

            yield return new WaitForSeconds(interval);
        }

        if (flameTrail != null)
            flameTrail.SetActive(false);

        isBurning = false;
        Debug.Log("🔥 Burn ended");
    }
    public void StopBurn()
    {
        if (isBurning && burnRoutine != null)
        {
            StopCoroutine(burnRoutine);
            isBurning = false;

            if (flameTrail != null)
                flameTrail.SetActive(false);
        }
    }
}