using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class GolemAI : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float patrolRadius = 4f;
    public float chaseRange = 10f;
    public float attackCooldown = 1.5f;
    public float attackDuration = 1.2f; // Extended for safer coroutine timing
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

    [Header("Attack Positioning")]
    public float desiredAttackDistance = 6.4f;
    public float distanceTolerance = 0.6f;
    public float verticalTolerance = 0.5f;

    [Header("Slam Attack")]
    public GameObject slamZonePrefab;

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
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        currentState = (distanceToPlayer <= chaseRange) ? State.Chase : State.Patrol;

        if (anim != null)
            anim.SetBool("IsWalking", currentState == State.Patrol || currentState == State.Chase);

        // TEMPORARY: Manual test trigger for slam
        if (Input.GetKeyDown(KeyCode.G))
        {
            Debug.Log("🧪 Test Slam Triggered");
            TriggerSlam();
        }
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
                Debug.Log("💥 AttackRoutine triggered");
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
        Vector2 direction = player.position - transform.position;
        float horizontalDistance = Mathf.Abs(direction.x);
        float verticalDistance = Mathf.Abs(direction.y);
        float moveDir = Mathf.Sign(direction.x);

        Vector2 targetPos = new Vector2(
            player.position.x - moveDir * desiredAttackDistance,
            player.position.y
        );

        if (horizontalDistance > desiredAttackDistance + distanceTolerance ||
            horizontalDistance < desiredAttackDistance - distanceTolerance ||
            verticalDistance > verticalTolerance)
        {
            MoveTo(targetPos);
        }
        else
        {
            rb.velocity = Vector2.zero;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = direction.x < 0;
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

        yield return new WaitForSeconds(attackDuration * 0.5f);
        Debug.Log("👊 TriggerSlam() called from coroutine");
        TriggerSlam();

        yield return new WaitForSeconds(attackDuration * 0.5f);

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        isAttacking = false;
    }

    public void TriggerSlam()
    {
        Debug.Log("🔥 TriggerSlam() executing");

        if (slamZonePrefab != null)
        {
            float offsetX = spriteRenderer.flipX ? -1f : 1f;
            Vector3 slamPosition = transform.position + new Vector3(offsetX, 1f, 0f);

            Instantiate(slamZonePrefab, slamPosition, Quaternion.identity);
        }
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

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, patrolRadius);

        Gizmos.color = Color.magenta;
        Vector3 idealLeft = transform.position + Vector3.left * desiredAttackDistance;
        Vector3 idealRight = transform.position + Vector3.right * desiredAttackDistance;
        Gizmos.DrawLine(transform.position, idealLeft);
        Gizmos.DrawLine(transform.position, idealRight);
    }
}
