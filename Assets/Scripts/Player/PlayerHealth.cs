using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 5;
    public int currentHealth;

    public UnityEvent OnDeath;
    public UnityEvent<int, int> OnHealthChanged; // current, max

    [Header("Passive Healing")]
    public bool enablePassiveHealing = true;
    public float healInterval = 5f;  // Time in seconds between heals
    public int healAmount = 1;       // How much to heal each tick
    private Coroutine passiveHealingRoutine;

    void Start()
    {
        Debug.Log("[PlayerHealth] Start()");
        currentHealth = Mathf.Clamp(currentHealth, 1, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (enablePassiveHealing)
        {
            passiveHealingRoutine = StartCoroutine(PassiveHeal());
        }

    }

    public void TakeDamage(int amount)
    {
        Animator anim = GetComponent<Animator>();
        if (anim != null)
            anim.SetTrigger("Hurt");

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    void Die()
    {
        Debug.Log("Player died.");
        if (passiveHealingRoutine != null)
            StopCoroutine(passiveHealingRoutine);
        // Freeze movement
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        // Play animation
        Animator anim = GetComponent<Animator>();
        if (anim != null)
            anim.SetTrigger("Die");

        // Lock movement
        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null)
            movement.canMove = false;

        // Lock combat
        PlayerCombat combat = GetComponent<PlayerCombat>();
        if (combat != null)
            combat.canAttack = false;

        // Trigger any extra listeners
        OnDeath?.Invoke();

        // Start cleanup
        StartCoroutine(DeathCleanup());
        SceneManager.LoadScene("FinalScene_Bad");
    }


    IEnumerator DeathCleanup()
    {
        // Wait for death animation (adjust as needed)
        yield return new WaitForSeconds(1.5f);

        Destroy(gameObject);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K)) // Press K to take damage
        {
            TakeDamage(1);
        }

        if (Input.GetKeyDown(KeyCode.H)) // Optional: Press H to heal
        {
            Heal(1);
        }
    }
    
    IEnumerator PassiveHeal()
    {
        while (true)
        {
            yield return new WaitForSeconds(healInterval);

            if (currentHealth < maxHealth)
            {
                Heal(healAmount);
                Debug.Log("[PlayerHealth] Passive heal applied: " + healAmount);
            }
        }
    }
}
