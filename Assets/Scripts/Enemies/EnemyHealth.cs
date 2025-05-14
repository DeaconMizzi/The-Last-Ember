using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 3;
    private int currentHealth;

    public EnemyType enemyType;
    private Animator animator;

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

        if (animator != null)
        {
            animator.SetTrigger("Death");
        }

        OrcAI ai = GetComponent<OrcAI>();
        if (ai != null) ai.enabled = false;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        Destroy(gameObject, 1.5f); // delay to allow death animation
    }
}