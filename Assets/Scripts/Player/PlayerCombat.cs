using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    private Animator animator;
    private float attackCooldownTimer = 0f;

    public bool canAttack = true;
    [Header("Attack Settings")]
    public float attackRange = 1f;
    public int attackDamage = 1;
    public float attackCooldown = 0.8f; // Adjust to match your animation length

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!canAttack)
        return;
        // Cooldown countdown
        if (attackCooldownTimer > 0f)
        {
            attackCooldownTimer -= Time.deltaTime;
        }

        // Attack input
        if (Input.GetKeyDown(KeyCode.Space) && attackCooldownTimer <= 0f)
        {
            Attack();
        }
    }

    void Attack()
    {
        animator.SetTrigger("Attack");
        attackCooldownTimer = attackCooldown;
    }

    public void DealDamage()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, attackRange);

        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                Debug.Log("Hit enemy: " + enemy.name);

                // Calculate knockback direction: from player to enemy
                Vector2 knockbackDirection = enemy.transform.position - transform.position;
                float knockbackForce = 5f; // you can tweak this per weapon or attack

                EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(attackDamage, knockbackDirection, knockbackForce);
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
