using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class OrcAI : MonoBehaviour, IDominionScalable, IStunnable, IStaggerable, IRetreatable
{
    public float moveSpeed = 2f;
    public float patrolRadius = 4f;
    public float chaseRange = 10f;
    public float attackCooldown = 1.5f;
    public float attackDuration = 0.6f;
    public int attackDamage = 1;
    public float retreatDuration = 1f;
    public float retreatDistance = 2f;
    public float circleSpeed = 1.5f;

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
    private bool isCircling = false;
    private bool isDead = false;
    private float circleAngle = 0f;

    private enum State { Patrol, Chase }
    private State currentState = State.Patrol;

    [Header("Directional Attack")]
    public float verticalTolerance = 0.5f;

    [Header("Attack Hitbox")]
    public GameObject attackHitbox;

    [Header("Attack Positioning")]
    public float desiredAttackDistance = 6.4f;
    public float distanceTolerance = 0.6f;

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
        if (isDead) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        currentState = (distanceToPlayer <= chaseRange) ? State.Chase : State.Patrol;

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

            float moveDir = Mathf.Sign(toPlayer.x);
            Vector2 targetPos = new Vector2(
                player.position.x - moveDir * desiredAttackDistance,
                player.position.y
            );

            if (!isCircling)
            {
                Vector2 dir = (transform.position - player.position).normalized;
                circleAngle = Mathf.Atan2(dir.y, dir.x);
                isCircling = true;
            }

            circleAngle += circleSpeed * Time.fixedDeltaTime;

            Vector2 orbitPos = new Vector2(
                player.position.x + Mathf.Cos(circleAngle) * desiredAttackDistance,
                player.position.y + Mathf.Sin(circleAngle) * desiredAttackDistance
            );

            Vector2 blendedTarget = Vector2.Lerp(orbitPos, targetPos, 0.5f);

            MoveTo(blendedTarget);

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

            if (!isAttacking && alignedHorizontally && verticallyClose && Time.time - lastAttackTime > attackCooldown)
            {
                StartCoroutine(AttackRoutine());
                lastAttackTime = Time.time;
            }
        }
        else
        {
            isCircling = false;
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

    void MoveTo(Vector2 target)
    {
        Vector2 direction = (target - (Vector2)transform.position).normalized;
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

        if (isDead) yield break;

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        isAttacking = false;

        TriggerRetreat();
    }

    public void TriggerRetreat()
    {
        if (isDead) return;
        StartCoroutine(RetreatRoutine());
    }

    private IEnumerator RetreatRoutine()
    {
        if (isDead) yield break;

        Vector2 retreatDirection = ((Vector2)transform.position - (Vector2)player.position).normalized;
        float timer = 0f;

        while (timer < retreatDuration)
        {
            if (isDead) yield break;
            rb.MovePosition(rb.position + retreatDirection * moveSpeed * Time.fixedDeltaTime);
            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
    }

    public void OnDeath()
    {
        if (isDead) return;
        isDead = true;

        rb.velocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;

        StopAllCoroutines(); // Stop all movement and attack coroutines

        if (anim != null)
        {
            anim.ResetTrigger("Attack");
            anim.SetTrigger("Death");
            anim.SetBool("IsWalking", false);
        }

        if (attackHitbox != null)
            attackHitbox.SetActive(false);
            
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

    public void Stagger(float duration)
    {
        if (isDead) return;

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
        if (isDead) yield break;

        yield return new WaitForSeconds(delay);

        if (isDead) yield break;

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    public void ApplyKnockback(float duration)
    {
        if (isDead) return;

        isKnockedBack = true;
        knockbackTimer = duration;
        TriggerRetreat();
    }

    private bool isStunned = false;

    public void Stun(float duration)
    {
        if (isDead) return;

        if (!isStunned)
            StartCoroutine(StunRoutine(duration));
    }

    private IEnumerator StunRoutine(float duration)
    {
        if (isDead) yield break;

        isStunned = true;
        rb.velocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        if (anim != null)
            anim.SetBool("IsWalking", false);

        yield return new WaitForSeconds(duration);

        if (isDead) yield break;

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        isStunned = false;
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
}