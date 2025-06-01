using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SkeletonAI : MonoBehaviour, IDominionScalable, IStunnable, IStaggerable, IRetreatable
{
    private bool isDead = false;
    public float moveSpeed = 2f;
    public float patrolRadius = 4f;
    public float chaseRange = 5f;
    public float attackCooldown = 1f;
    public float attackDuration = 1.1f;
    public int attackDamage = 1;
    public float retreatDuration = 1f;
    public float retreatDistance = 2f;
    public float invulnerabilityDuration = 0.5f;

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
    private bool isInvulnerable = false;

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

        if (newState != State.Chase && isCircling)
        {
            isCircling = false;
            circleAngle = 0f;
        }

        if (newState == State.Chase && !isCircling)
        {
            Vector2 dir = (transform.position - player.position).normalized;
            circleAngle = Mathf.Atan2(dir.y, dir.x);
            isCircling = true;
        }

        currentState = newState;

        if (anim != null)
            anim.SetBool("IsWalking", currentState == State.Patrol || currentState == State.Chase);
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

        circleAngle += circleSpeed * Time.fixedDeltaTime;

        Vector2 orbitPos = new Vector2(
            player.position.x + Mathf.Cos(circleAngle) * circleRadius,
            player.position.y + Mathf.Sin(circleAngle) * circleRadius
        );

        MoveTo(orbitPos);

        if (spriteRenderer != null)
            spriteRenderer.flipX = (player.position.x < transform.position.x);

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

        TriggerRetreat();
    }

    public void TriggerRetreat()
    {
        StartCoroutine(RetreatRoutine());
    }

    private IEnumerator RetreatRoutine()
    {
        Vector2 retreatDirection = ((Vector2)transform.position - (Vector2)player.position).normalized;
        float timer = 0f;

        while (timer < retreatDuration)
        {
            rb.MovePosition(rb.position + retreatDirection * moveSpeed * Time.fixedDeltaTime);
            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
    }

    public void ApplyInvulnerability()
    {
        StartCoroutine(InvulnerabilityRoutine());
    }

    private IEnumerator InvulnerabilityRoutine()
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(invulnerabilityDuration);
        isInvulnerable = false;
    }

    public void Stagger(float duration)
    {
        if (!isAttacking && !isKnockedBack)
        {
            rb.velocity = Vector2.zero;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
            StartCoroutine(ResumeAfterStagger(duration));
            TriggerRetreat();
        }
    }

    private IEnumerator ResumeAfterStagger(float delay)
    {
        yield return new WaitForSeconds(delay);
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    public void ApplyKnockback(float duration)
    {
        isKnockedBack = true;
        knockbackTimer = duration;
        TriggerRetreat();
    }

    public void PlayHurtAnimation()
    {
        if (anim != null)
        {
            anim.ResetTrigger("Hurt");
            anim.SetTrigger("Hurt");
        }
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

    public void Stun(float duration)
    {
        if (!isKnockedBack)
            StartCoroutine(StunRoutine(duration));
    }

    private IEnumerator StunRoutine(float duration)
    {
        rb.velocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        if (anim != null)
            anim.SetBool("IsWalking", false);

        yield return new WaitForSeconds(duration);

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    public void EnableHitbox()
    {
        if (isDead) return;

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
        if (attackHitbox != null)
            attackHitbox.SetActive(false);
    }
    public void OnDeath()
    {
        if (isDead) return;
        isDead = true;

        // Freeze rigidbody
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        // Stop AI behavior
        StopAllCoroutines();

        // Play death animation
        if (anim != null)
        {
            anim.SetBool("Death", true);
            anim.SetBool("IsWalking", false);
        }

        // Disable attack hitbox
        if (attackHitbox != null)
            attackHitbox.SetActive(false);
    }
}