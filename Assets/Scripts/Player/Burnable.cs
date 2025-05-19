using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Burnable : MonoBehaviour
{
    public float burnDamagePerTick = 0.2f;
    public float burnDuration = 5f;
    public float burnTickRate = 1f;
    public GameObject flameTrail; // Assign looping visual FX (optional)

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
        float interval = burnDuration / ticks; // 5s / 2 = 2.5s

        for (int i = 0; i < ticks; i++)
        {
            // Damage
            PlayerHealth health = GetComponent<PlayerHealth>();
            if (health != null)
                health.TakeDamage(Mathf.CeilToInt(burnDamagePerTick)); // use 0.1 or round to 1

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