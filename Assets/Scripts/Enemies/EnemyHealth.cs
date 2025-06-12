using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 3;
    public int currentHealth;
    private bool isDead = false;
    private bool isInvulnerable = false;
    public float invulnerabilityDuration = 0.5f;
    public EnemyType enemyType;
    private Animator animator;

    [Header("Boss Flags")]
    public bool isBranhalm = false;
    [Header("Death Settings")]
    public float destroyDelay = 1.5f;
    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
    }

   public void TakeDamage(int amount, Vector2 knockbackDirection, float knockbackForce)
    {
        if (isInvulnerable) return;

        currentHealth -= amount;
        Debug.Log($"{enemyType} took {amount} damage!");

        GetComponent<Rigidbody2D>()?.AddForce(knockbackDirection.normalized * knockbackForce, ForceMode2D.Impulse);

        IStaggerable staggerTarget = GetComponent<IStaggerable>();
        if (staggerTarget != null)
        {
            staggerTarget.Stagger(1f);
        }

        StartCoroutine(InvulnerabilityRoutine());
        GetComponent<Animator>()?.SetTrigger("Hurt");

        if (currentHealth <= 0) Die();
    }

    private IEnumerator InvulnerabilityRoutine()
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(invulnerabilityDuration);
        isInvulnerable = false;
    }
    void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log($"{enemyType} died!");
        QuestManager.Instance.RegisterEnemyKill(enemyType);

        if (isBranhalm)
        {
            MemoryFlags.Set("BRANHALM_DEFEATED");
            Debug.Log("Branhalm defeated — MemoryFlag set.");
        }

        if (animator != null)
        {
            animator.SetTrigger("Death");
        }

        // Freeze rigidbody completely
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        // Disable colliders
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // Tell AI scripts to stop
        SendMessage("OnDeath", SendMessageOptions.DontRequireReceiver);

        Destroy(gameObject, destroyDelay);
    }
}
