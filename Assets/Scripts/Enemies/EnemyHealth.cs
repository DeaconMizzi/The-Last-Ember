using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 3;
    private int currentHealth;

    public EnemyType enemyType;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount, Vector2 knockbackDirection, float knockbackForce)
    {
        currentHealth -= amount;
        Debug.Log($"{enemyType} took {amount} damage!");
        GetComponent<Rigidbody2D>()?.AddForce(knockbackDirection.normalized * knockbackForce, ForceMode2D.Impulse);
        OrcAI ai = GetComponent<OrcAI>();
        if (ai != null)
        {
            ai.ApplyKnockback(0.25f); // match knockbackDuration
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
        Destroy(gameObject);
    }
}