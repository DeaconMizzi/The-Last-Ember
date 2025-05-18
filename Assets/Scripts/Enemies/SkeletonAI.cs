using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SkeletonAI : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float patrolRadius = 4f;
    public float chaseRange = 5f;
    public float attackCooldown = 1f;
    public float attackDuration = 1.1f;
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

    [Header("Directional Attack")]
    public float verticalTolerance = 0.5f;

    [Header("Attack Hitbox")]
    public GameObject attackHitbox;

    [Header("Attack Positioning")]
    public float desiredAttackDistance = 1.5f;
    public float distanceTolerance = 0.4f;

    private Vector3 baseHitboxOffset;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        startPosition = transform.position;
        patrolTarget = GetNewPatrolPoint();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        if (attackHitbox != null)
            baseHitboxOffset = attackHitbox.transform.localPosition;
    }

    void Update()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
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
            Vector2 toPlayer = player.position - transform.position;
            float horizontalDist = Mathf.Abs(toPlayer.x);
            float verticalDist = Mathf.Abs(toPlayer.y);

            bool alignedHorizontally = horizontalDist <= desiredAttackDistance + distanceTolerance;
            bool verticallyClose = verticalDist <= verticalTolerance;

            ChasePlayer(toPlayer);

            if (!isAttacking && alignedHorizontally && verticallyClose && Time.time - lastAttackTime > attackCooldown)
            {
                Debug.Log("🔁 Triggering Jab Animation");
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

    void ChasePlayer(Vector2 toPlayer)
    {
        float horizontalDistance = Mathf.Abs(toPlayer.x);
        float verticalDistance = Mathf.Abs(toPlayer.y);

        // Sprite direction determines flank direction
        bool facingLeft = spriteRenderer != null && spriteRenderer.flipX;
        float flankDir = facingLeft ? 1f : -1f;

        // Desired position to attack from (flanking around player)
        Vector2 desiredPosition = new Vector2(
            player.position.x + flankDir * desiredAttackDistance,
            player.position.y
        );

        // Check if we're already on the correct side (flanked)
        bool onCorrectFlankSide = (flankDir > 0 && transform.position.x > player.position.x) ||
                                (flankDir < 0 && transform.position.x < player.position.x);

        // Step 1: Move to correct flank side first
        if (!onCorrectFlankSide)
        {
            Vector2 target = new Vector2(player.position.x + flankDir * desiredAttackDistance, transform.position.y);
            MoveTo(target);
            return;
        }

        // Step 2: Align vertically
        if (verticalDistance > verticalTolerance)
        {
            Vector2 target = new Vector2(transform.position.x, player.position.y);
            MoveTo(target);
            return;
        }

        // Step 3: Move into final attack position (if not already)
        float closeEnough = distanceTolerance;
        if (Vector2.Distance(transform.position, desiredPosition) > closeEnough)
        {
            MoveTo(desiredPosition);
        }
        else
        {
            rb.velocity = Vector2.zero;
        }

        // Sprite facing update
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = toPlayer.x < 0;
        }

        // Hitbox flip logic
        if (attackHitbox != null)
        {
            Vector3 scale = attackHitbox.transform.localScale;
            scale.x = spriteRenderer.flipX ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
            attackHitbox.transform.localScale = scale;

            Vector3 flippedOffset = baseHitboxOffset;
            flippedOffset.x = spriteRenderer.flipX ? -Mathf.Abs(baseHitboxOffset.x) : Mathf.Abs(baseHitboxOffset.x);
            attackHitbox.transform.localPosition = flippedOffset;
        }
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

        yield return new WaitForSeconds(attackDuration);

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        isAttacking = false;
    }

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

    public void PlayHurtAnimation()
    {
        if (anim != null)
        {
            anim.ResetTrigger("Hurt");
            anim.SetTrigger("Hurt");
            StartCoroutine(ResetHurtTrigger());
        }
    }

    private IEnumerator ResetHurtTrigger()
    {
        yield return new WaitForSeconds(0.6f); // Match actual hurt animation length
        anim.ResetTrigger("Hurt");
    }

    public void ApplyKnockback(float duration)
    {
        isKnockedBack = true;
        knockbackTimer = duration;

        PlayHurtAnimation(); // Trigger hurt visual
    }

    public void PlayDeathAnimation()
    {
        if (anim != null)
        {
            anim.SetBool("Death", true);
            rb.velocity = Vector2.zero;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        // Optionally disable the AI logic after a delay
        StartCoroutine(HandleDeath());
    }

    private IEnumerator HandleDeath()
    {
        yield return new WaitForSeconds(1.2f); // Match death animation time
        Destroy(gameObject); // Or pool/disable instead
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, patrolRadius);

        Gizmos.color = Color.magenta;
        Vector3 idealLeft = transform.position + Vector3.left * desiredAttackDistance;
        Vector3 idealRight = transform.position + Vector3.right * desiredAttackDistance;
        Gizmos.DrawLine(transform.position, idealLeft);
        Gizmos.DrawLine(transform.position, idealRight);

        if (spriteRenderer != null && attackHitbox != null)
        {
            Gizmos.color = Color.cyan;
            Vector3 center = attackHitbox.transform.position;
            BoxCollider2D col = attackHitbox.GetComponent<BoxCollider2D>();
            if (col != null)
                Gizmos.DrawWireCube(center, col.size);
        }
    }
}
