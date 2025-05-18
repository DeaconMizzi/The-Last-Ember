using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 3;
    private int currentHealth;

    public EnemyType enemyType;
    private Animator animator;

    [Header("Boss Flags")]
    public bool isBranhalm = false;

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
    }

    public void TakeDamage(int amount, Vector2 knockbackDirection, float knockbackForce)
    {
        currentHealth -= amount;
        Debug.Log($"{enemyType} took {amount} damage!");
        GetComponent<Rigidbody2D>()?.AddForce(knockbackDirection.normalized * knockbackForce, ForceMode2D.Impulse);

        OrcAI ai = GetComponent<OrcAI>();
        if (ai != null)
        {
            ai.ApplyKnockback(0.25f);
        }

        if (animator != null)
        {
            animator.SetTrigger("Hurt");
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log($"{enemyType} died!");
        QuestManager.Instance.RegisterEnemyKill(enemyType);

        if (isBranhalm)
        {
            MemoryFlags.Set("BRANHALM_DEFEATED");
            Debug.Log("Branhalm defeated — MemoryFlag set.");
        }

        // Trigger animator death animation
        if (animator != null)
        {
            animator.SetTrigger("Death");
        }

        // Disable AI (for Orcs)
        OrcAI orcAI = GetComponent<OrcAI>();
        if (orcAI != null) orcAI.enabled = false;

        // Disable AI (for Skeletons)
        SkeletonAI skeletonAI = GetComponent<SkeletonAI>();
        if (skeletonAI != null)
        {
            skeletonAI.PlayDeathAnimation();
            return; // Let SkeletonAI handle the destroy
        }

        // Disable collider
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // Stop all physics movement
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        // Destroy after delay to allow death animation
        Destroy(gameObject, 1.5f);
    }

}
