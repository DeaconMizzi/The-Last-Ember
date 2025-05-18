using System.Collections;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    private Animator animator;
    private float attackCooldownTimer = 0f;
    private Vector2 lastMoveDir = Vector2.down; // Default direction

    public bool canAttack = true;

    [Header("Attack Settings")]
    public float attackCooldown = 0.8f;
    public float hitboxActiveTime = 0.2f;
    public int attackDamage = 1;
    public float knockbackForce = 5f;

    [Header("Directional Hitboxes")]
    public GameObject hitboxUp;
    public GameObject hitboxDown;
    public GameObject hitboxLeft;
    public GameObject hitboxRight;

    private GameObject currentActiveHitbox;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!canAttack)
            return;

        // Track last movement direction
        Vector2 moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (moveInput != Vector2.zero)
            lastMoveDir = moveInput.normalized;

        // Cooldown countdown
        if (attackCooldownTimer > 0f)
            attackCooldownTimer -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Space) && attackCooldownTimer <= 0f)
        {
            TriggerAttack();
        }
    }

    void TriggerAttack()
    {
        animator.SetTrigger("Attack");
        attackCooldownTimer = attackCooldown;

        Vector2 dir = lastMoveDir;

        // Disable all hitboxes first
        hitboxUp.SetActive(false);
        hitboxDown.SetActive(false);
        hitboxLeft.SetActive(false);
        hitboxRight.SetActive(false);

        if (dir.y > 0)
            currentActiveHitbox = hitboxUp;
        else if (dir.y < 0)
            currentActiveHitbox = hitboxDown;
        else if (dir.x < 0)
            currentActiveHitbox = hitboxLeft;
        else
            currentActiveHitbox = hitboxRight;

        if (currentActiveHitbox != null)
            currentActiveHitbox.SetActive(true);

        StartCoroutine(DisableHitboxAfterDelay(hitboxActiveTime));
    }

    private IEnumerator DisableHitboxAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        hitboxUp.SetActive(false);
        hitboxDown.SetActive(false);
        hitboxLeft.SetActive(false);
        hitboxRight.SetActive(false);
        currentActiveHitbox = null;
    }

    // Animation event trigger
    public void DealDamage()
    {
        if (currentActiveHitbox == null)
        {
            Debug.LogWarning("No active hitbox during DealDamage call.");
            return;
        }

        Collider2D[] hits = Physics2D.OverlapBoxAll(currentActiveHitbox.transform.position,
                                                    currentActiveHitbox.GetComponent<BoxCollider2D>().size,
                                                    0f);

        foreach (Collider2D enemy in hits)
        {
            if (enemy.CompareTag("Enemy"))
            {
                Debug.Log("Hit enemy: " + enemy.name);

                Vector2 knockbackDir = (enemy.transform.position - transform.position).normalized;
                EnemyHealth health = enemy.GetComponent<EnemyHealth>();

                if (health != null)
                    health.TakeDamage(attackDamage, knockbackDir, knockbackForce);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (hitboxUp != null)
            Gizmos.DrawWireCube(hitboxUp.transform.position, hitboxUp.GetComponent<BoxCollider2D>().size);
        if (hitboxDown != null)
            Gizmos.DrawWireCube(hitboxDown.transform.position, hitboxDown.GetComponent<BoxCollider2D>().size);
        if (hitboxLeft != null)
            Gizmos.DrawWireCube(hitboxLeft.transform.position, hitboxLeft.GetComponent<BoxCollider2D>().size);
        if (hitboxRight != null)
            Gizmos.DrawWireCube(hitboxRight.transform.position, hitboxRight.GetComponent<BoxCollider2D>().size);
    }
}
