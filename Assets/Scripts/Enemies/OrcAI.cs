using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class OrcAI : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float patrolRadius = 4f;
    public float chaseRange = 5f;
    public float attackRange = 1f;
    public float attackCooldown = 1.5f;
    public float attackDuration = 0.6f;
    public int attackDamage = 1;

    private float lastAttackTime = 0f;
    private Vector2 patrolTarget;
    private Vector2 startPosition;
    private Rigidbody2D rb;
    private Transform player;
    private Animator anim;
    private SpriteRenderer spriteRenderer;

    private bool isKnockedBack = false;
    private float knockbackTimer = 0f;
    private bool isAttacking = false;

    private enum State { Patrol, Chase }
    private State currentState = State.Patrol;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        startPosition = transform.position;
        patrolTarget = GetNewPatrolPoint();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    void Update()
    {
        float distanceToPlayer = Vector2.Distance((Vector2)transform.position, (Vector2)player.position);
        currentState = (distanceToPlayer <= chaseRange) ? State.Chase : State.Patrol;

        if (anim != null)
            anim.SetBool("IsWalking", currentState == State.Patrol || currentState == State.Chase);
    }

    void FixedUpdate()
    {
        if (isKnockedBack)
        {
            knockbackTimer -= Time.fixedDeltaTime;
            if (knockbackTimer <= 0f)
                isKnockedBack = false;
            return;
        }

        if (isAttacking) return;

        if (currentState == State.Chase)
        {
            ChasePlayer();

            float distance = Vector2.Distance((Vector2)transform.position, (Vector2)player.position);
            Debug.DrawLine(transform.position, player.position, Color.magenta);
            Debug.Log("Distance to player: " + distance);
            if (distance <= attackRange && Time.time - lastAttackTime > attackCooldown)
            {
                Debug.Log("In attack range!");
                StartCoroutine(AttackRoutine());
                lastAttackTime = Time.time;
            }
        }
        else
        {
            Patrol();
        }
    }

    void Patrol()
    {
        if (Vector2.Distance(transform.position, patrolTarget) < 0.5f)
        {
            patrolTarget = GetNewPatrolPoint();
        }

        MoveTo(patrolTarget);
    }
    Vector2 GetNewPatrolPoint()
    {
        Vector2 offset = Random.insideUnitCircle * patrolRadius;
        return startPosition + offset;
    }
    void ChasePlayer()
    {
        MoveTo(player.position);
    }

    void MoveTo(Vector2 target)
    {
        Vector2 direction = (target - (Vector2)transform.position).normalized;

        if (spriteRenderer != null && direction.x != 0)
            spriteRenderer.flipX = direction.x < 0;

        rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime);
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;

        rb.velocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;

        if (anim != null)
            anim.SetTrigger("Attack");

        // ✅ Wait for wind-up
        yield return new WaitForSeconds(0.8f);

        // ✅ Check distance again before applying damage
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer <= attackRange)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
                playerHealth.TakeDamage(attackDamage);
        }

        // Finish animation
        yield return new WaitForSeconds(attackDuration - 0.8f);

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        isAttacking = false;
    }

    public void ApplyKnockback(float duration)
    {
        isKnockedBack = true;
        knockbackTimer = duration;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, patrolRadius);
    }
}
