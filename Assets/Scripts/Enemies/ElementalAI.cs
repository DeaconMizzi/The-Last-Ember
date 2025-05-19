using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ElementalAI : MonoBehaviour
{
    [Header("Movement & Ranges")]
    public float moveSpeed = 1.5f;
    public float patrolRadius = 2f;
    public float chaseRange = 3f;
    public float attackRange = 1.2f;

    [Header("Combat Settings")]
    public float attackCooldown = 2f;
    public float attackWindupTime = 0.4f;
    public int attackDamage = 1;
    public float knockbackForce = 5f;

    [Header("Attack Hitbox")]
    public GameObject attackHitbox; // Assigned in Inspector
    private Vector3 baseHitboxOffset;
    private SpriteRenderer spriteRenderer;

    private Rigidbody2D rb;
    private Transform player;
    private Animator anim;
    private Vector2 startPosition;
    private Vector2 patrolTarget;
    private float lastAttackTime = -999f;
    private bool hasDealtDamageThisSwing = false;
    private enum State { Idle, Patrol, Chase, Attack }
    private State currentState = State.Patrol;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        startPosition = transform.position;
        patrolTarget = GetNewPatrolPoint();

        if (attackHitbox != null)
            baseHitboxOffset = attackHitbox.transform.localPosition;
    }

    void Update()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange && Time.time - lastAttackTime > attackCooldown)
        {
            currentState = State.Attack;
        }
        else if (distanceToPlayer <= chaseRange)
        {
            currentState = State.Chase;
        }
        else
        {
            currentState = State.Patrol;
        }

        anim.SetBool("IsWalking", currentState == State.Chase || currentState == State.Patrol);

        if (spriteRenderer != null && player != null)
        {
            spriteRenderer.flipX = (player.position.x < transform.position.x);
        }
    }

    void FixedUpdate()
    {
        if (currentState == State.Patrol)
            Patrol();
        else if (currentState == State.Chase)
            Chase();
        else if (currentState == State.Attack)
            StartCoroutine(Attack());
    }

    void Patrol()
    {
        if (Vector2.Distance(transform.position, patrolTarget) < 0.2f)
        {
            patrolTarget = GetNewPatrolPoint();
        }

        MoveTo(patrolTarget);
    }

    void Chase()
    {
        MoveTo(player.position);
    }

    Vector2 GetNewPatrolPoint()
    {
        return startPosition + Random.insideUnitCircle * patrolRadius;
    }

    void MoveTo(Vector2 target)
    {
        Vector2 dir = (target - (Vector2)transform.position).normalized;
        rb.MovePosition(rb.position + dir * moveSpeed * Time.fixedDeltaTime);
    }

    IEnumerator Attack()
    {
        currentState = State.Idle;
        rb.velocity = Vector2.zero;
        anim.SetTrigger("Attack");
        yield return new WaitForSeconds(attackWindupTime);
        lastAttackTime = Time.time;
        hasDealtDamageThisSwing = false;
    }

    // Triggered by animation event
    public void DealDamage()
    {
        if (hasDealtDamageThisSwing)
            return;

        hasDealtDamageThisSwing = true;

        Vector2 center = transform.position;
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, attackRange);

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                // Knockback
                Vector2 knockbackDir = (hit.transform.position - transform.position).normalized;
                Rigidbody2D prb = hit.GetComponent<Rigidbody2D>();
                if (prb != null)
                    prb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);

                // Burn effect
                Burnable burn = hit.GetComponent<Burnable>();
                if (burn != null)
                    burn.ApplyBurn();

                // Damage
                PlayerHealth health = hit.GetComponent<PlayerHealth>();
                if (health != null)
                    health.TakeDamage(attackDamage);

                Debug.Log("🔥 Elemental hit player with burn + knockback.");
            }
        }
    }


    // Animation events
    public void EnableHitbox()
    {
        Debug.Log("✅ Hitbox Enabled");
        if (attackHitbox != null)
        {
            attackHitbox.SetActive(true);

            var hitboxScript = attackHitbox.GetComponent<EnemyWeaponHitbox>();
            if (hitboxScript != null)
                hitboxScript.ResetHit();
        }
    }

    public void DisableHitbox()
    {
        Debug.Log("❌ Hitbox Disabled");
        if (attackHitbox != null)
            attackHitbox.SetActive(false);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, patrolRadius);
    }
}