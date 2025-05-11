using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 5;
    public int currentHealth;

    public UnityEvent OnDeath;
    public UnityEvent<int, int> OnHealthChanged; // current, max

    void Start()
    {
        Debug.Log("[PlayerHealth] Start()");
        currentHealth = Mathf.Clamp(currentHealth, 1, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
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
}
