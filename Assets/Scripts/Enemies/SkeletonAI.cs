using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SkeletonAI : MonoBehaviour, IDominionScalable, IStunnable, IStaggerable
{
    private bool isDead = false;
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

    // --- Curve-based movement additions ---
    private bool isCircling = false;
    private float circleAngle = 0f;
    public float circleSpeed = 1.5f;
    public float circleRadius = 2f;

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
         if (isDead) return;
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        State newState = (distanceToPlayer <= chaseRange) ? State.Chase : State.Patrol;

        // Reset circling when leaving chase
        if (newState != State.Chase && isCircling)
        {
            isCircling = false;
            circleAngle = 0f;
        }

        // Start circling
        if (newState == State.Chase && !isCircling)
        {
            Vector2 dir = (transform.position - player.position).normalized;
            circleAngle = Mathf.Atan2(dir.y, dir.x); // angle in radians
            isCircling = true;
        }

        currentState = newState;

        if (anim != null)
            anim.SetBool("IsWalking", currentState == State.Patrol || currentState == State.Chase);
    }
    public void Stagger(float duration)
    {
        if (!isAttacking && !isKnockedBack)
        {
            rb.velocity = Vector2.zero;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
            StartCoroutine(ResumeAfterStagger(duration));
        }
    }

    private IEnumerator ResumeAfterStagger(float delay)
    {
        yield return new WaitForSeconds(delay);
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }
    void FixedUpdate()
    {
         if (isDead) return;
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
        if (!isCircling) return;

        // Step around the player in a circular arc
        circleAngle += circleSpeed * Time.fixedDeltaTime;

        Vector2 orbitPos = new Vector2(
            player.position.x + Mathf.Cos(circleAngle) * circleRadius,
            player.position.y + Mathf.Sin(circleAngle) * circleRadius
        );

        MoveTo(orbitPos);

        // Flip sprite for facing
        if (spriteRenderer != null)
            spriteRenderer.flipX = (player.position.x < transform.position.x);

        // Flip and offset hitbox
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
        if (isDead) yield break;
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
        if (isDead) return; // ✅ Prevent multiple calls
        isDead = true;

        if (anim != null)
        {
            anim.SetBool("Death", true);
            rb.velocity = Vector2.zero;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        StartCoroutine(HandleDeath());
    }

    private IEnumerator HandleDeath()
    {
        yield return new WaitForSeconds(1f);
        Destroy(gameObject); // Or disable for pooling
    }

    void OnDrawGizmosSelected()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, patrolRadius);

        // Attack position lines
        Gizmos.color = Color.magenta;
        Vector3 idealLeft = transform.position + Vector3.left * desiredAttackDistance;
        Vector3 idealRight = transform.position + Vector3.right * desiredAttackDistance;
        Gizmos.DrawLine(transform.position, idealLeft);
        Gizmos.DrawLine(transform.position, idealRight);

        // Hitbox
        if (attackHitbox != null)
        {
            Gizmos.color = Color.cyan;
            Vector3 center = attackHitbox.transform.position;
            BoxCollider2D col = attackHitbox.GetComponent<BoxCollider2D>();
            if (col != null)
                Gizmos.DrawWireCube(center, col.size);
        }

#if UNITY_EDITOR
        // Try to find player if not set
        if (player == null)
        {
            GameObject found = GameObject.FindGameObjectWithTag("Player");
            if (found != null) player = found.transform;
        }

        if (player != null)
        {
            // Orbit circle
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.7f);
            const int segments = 32;
            Vector3 lastPoint = player.position + new Vector3(Mathf.Cos(0) * circleRadius, Mathf.Sin(0) * circleRadius, 0);
            for (int i = 1; i <= segments; i++)
            {
                float theta = i * Mathf.PI * 2f / segments;
                Vector3 nextPoint = player.position + new Vector3(Mathf.Cos(theta) * circleRadius, Mathf.Sin(theta) * circleRadius, 0);
                Gizmos.DrawLine(lastPoint, nextPoint);
                lastPoint = nextPoint;
            }

            // Target orbit point
            Gizmos.color = Color.red;
            Vector3 target = player.position + new Vector3(Mathf.Cos(circleAngle), Mathf.Sin(circleAngle), 0) * circleRadius;
            Gizmos.DrawSphere(target, 0.1f);

            // Attack distance
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(player.position, desiredAttackDistance);

            // Vertical tolerance band
            Gizmos.color = new Color(0f, 0.8f, 1f, 0.4f);
            float yMin = player.position.y - verticalTolerance;
            float yMax = player.position.y + verticalTolerance;
            Vector3 bandStart = new Vector3(player.position.x - desiredAttackDistance * 2, yMin, 0);
            Vector3 bandEnd = new Vector3(player.position.x + desiredAttackDistance * 2, yMax, 0);
            Gizmos.DrawLine(new Vector3(bandStart.x, yMin, 0), new Vector3(bandEnd.x, yMin, 0));
            Gizmos.DrawLine(new Vector3(bandStart.x, yMax, 0), new Vector3(bandEnd.x, yMax, 0));
        }
#endif
    }
    public void ApplyReforgedScaling()
    {
        attackDamage += 1;

        var health = GetComponent<EnemyHealth>();
        if (health != null)
        {
            health.maxHealth += 2;
            health.currentHealth += 2;
        }
    }
    private bool isStunned = false;

    public void Stun(float duration)
    {
        if (!isStunned)
            StartCoroutine(StunRoutine(duration));
    }

    private IEnumerator StunRoutine(float duration)
    {
        isStunned = true;
        rb.velocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        if (anim != null)
            anim.SetBool("IsWalking", false);

        yield return new WaitForSeconds(duration);

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        isStunned = false;
    }

}
