using System.Collections;
using UnityEngine;

public class PlayerAbilities : MonoBehaviour
{
    private PlayerMovement playerMovement;
    [Header("Bloomstep")]
    public GameObject flowerTrailPrefab;
    public GameObject ghostTrailPrefab;
    public Vector2 flowerSpawnOffset = new Vector2(0f, -0.5f);
    private bool canBloomstep = false;
    private bool bloomstepOnCooldown = false;
    private CameraShaker cameraShaker;
    private Rigidbody2D rb;
    private Collider2D playerCollider;

    [Header("Command Pulse")]
    private bool hasCommandPulse = false;
    private bool pulseOnCooldown = false;

    [Header("Command Pulse Settings")]
    public float pulseRadius = 2f;
    public float stunDuration = 1.5f;
    public LayerMask enemyLayer;
    public GameObject pulseVFXPrefab;

    [Header("Flame Guard")]
    private bool hasFlameGuard = false;
    private bool flameGuardOnCooldown = false;
    public GameObject flameShieldObject;
    public float flameGuardDuration = 3f;
    public float flameGuardCooldown = 6f;
    public int contactDamage = 1;

    [Header("Pulse FX")]
    public GameObject dustFX;
    public Vector3 dustFXPosition = new Vector3(-0.278f, -0.901f, 0f);

    [Header("Control UI References")]
    public GameObject bloomstepControlText;
    public GameObject flameguardControlText;
    public GameObject commandpulseControlText;

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        if (playerCollider == null)
        {
            Debug.LogWarning("No Collider2D found on Player. Dash collision checks will not work!");
        }

        cameraShaker = GameObject.Find("CmCam")?.GetComponent<CameraShaker>();
        if (cameraShaker == null) Debug.LogWarning("CameraShaker not found.");
    }

    void Update()
    {
        HandleBloomstep();
        HandleCommandPulse();
        HandleFlameGuard();
    }

    public void EnableBloomstep()
    {
        canBloomstep = true;
        if (bloomstepControlText != null) bloomstepControlText.SetActive(true);
        Debug.Log("Bloomstep unlocked!");
    }

    public void EnableFlameGuard()
    {
        hasFlameGuard = true;
        if (flameguardControlText != null) flameguardControlText.SetActive(true);
        Debug.Log("Flame Guard unlocked!");
    }

    public void EnableCommandPulse()
    {
        hasCommandPulse = true;
        if (commandpulseControlText != null) commandpulseControlText.SetActive(true);
        Debug.Log("Command Pulse unlocked!");
    }

    void HandleBloomstep()
    {
        if (!canBloomstep || bloomstepOnCooldown) return;

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            Vector2 dashDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

            if (dashDirection == Vector2.zero && playerMovement != null)
            {
                dashDirection = playerMovement.LastMoveDirection;
            }

            if (dashDirection == Vector2.zero)
            {
                dashDirection = Vector2.right; // Fallback default
            }

            float dashDistance = 2.5f;
            float dashDuration = 0.2f;

            Debug.Log("Bloomstep dash start!");
            StartCoroutine(SmoothDash(dashDirection, dashDistance, dashDuration));
            StartCoroutine(BloomstepCooldown(1.5f));
        }
    }

    IEnumerator SmoothDash(Vector2 direction, float distance, float duration)
    {
        if (playerCollider == null)
        {
            Debug.LogWarning("No Collider2D found on Player. Skipping dash collision checks.");
            yield break;
        }

        if (playerMovement != null)
            playerMovement.isDashing = true;

        float elapsed = 0f;
        float dashSpeed = distance / duration;
        float stepDistance = dashSpeed * Time.fixedDeltaTime;

        ContactFilter2D contactFilter = new ContactFilter2D().NoFilter();
        contactFilter.useLayerMask = true;
        contactFilter.layerMask = LayerMask.GetMask("Default", "Obstacles");
        contactFilter.useTriggers = false;

        while (elapsed < duration)
        {
            Vector2 moveVector = direction.normalized * stepDistance;

            RaycastHit2D[] results = new RaycastHit2D[1];
            int hitCount = playerCollider.Cast(direction, contactFilter, results, stepDistance);

            if (hitCount > 0)
            {
                Debug.Log($"Bloomstep stopped by {results[0].collider.name}");
                break;
            }

            rb.MovePosition(rb.position + moveVector);

            SpawnTrail();
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        if (playerMovement != null)
            playerMovement.isDashing = false;
    }

    IEnumerator BloomstepCooldown(float duration)
    {
        bloomstepOnCooldown = true;
        yield return new WaitForSeconds(duration);
        bloomstepOnCooldown = false;
    }

    void SpawnTrail()
    {
        Vector2 randomOffset = new Vector2(Random.Range(-0.05f, 0.05f), Random.Range(-0.1f, 0.1f));
        Vector3 flowerPos = rb.position + flowerSpawnOffset + randomOffset;
        GameObject flower = Instantiate(flowerTrailPrefab, flowerPos, Quaternion.identity);

        SpriteRenderer playerSR = GetComponent<SpriteRenderer>();
        if (playerSR != null && flower.GetComponent<SpriteRenderer>() is SpriteRenderer flowerSR)
        {
            flowerSR.sortingLayerID = playerSR.sortingLayerID;
            flowerSR.sortingOrder = playerSR.sortingOrder - 2;
        }

        if (ghostTrailPrefab != null)
        {
            GameObject ghost = Instantiate(ghostTrailPrefab, rb.position, Quaternion.identity);
            if (ghost.GetComponent<SpriteRenderer>() is SpriteRenderer ghostSR && playerSR != null)
            {
                ghostSR.sprite = playerSR.sprite;
                ghostSR.flipX = playerSR.flipX;
                ghostSR.sortingLayerID = playerSR.sortingLayerID;
                ghostSR.sortingOrder = playerSR.sortingOrder - 1;
            }
        }
    }

    void HandleCommandPulse()
    {
        if (!hasCommandPulse || pulseOnCooldown) return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (cameraShaker != null) cameraShaker.Shake(6f, 0.3f);
            Debug.Log("Command Pulse Activated!");

            if (pulseVFXPrefab != null) Instantiate(pulseVFXPrefab, transform.position, Quaternion.identity);

            if (dustFX != null)
            {
                dustFX.transform.position = transform.position + dustFXPosition;
                dustFX.SetActive(true);
                StartCoroutine(DisableFXAfterDelay(dustFX, 0.7f));
            }

            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, pulseRadius, enemyLayer);
            foreach (Collider2D hit in hits)
            {
                IStunnable target = hit.GetComponent<IStunnable>();
                if (target != null)
                {
                    target.Stun(stunDuration);
                    Debug.Log($"Stunned: {hit.name}");
                }
            }

            StartCoroutine(PulseCooldown(5f));
        }
    }

    IEnumerator PulseCooldown(float duration)
    {
        pulseOnCooldown = true;
        yield return new WaitForSeconds(duration);
        pulseOnCooldown = false;
    }

    private IEnumerator DisableFXAfterDelay(GameObject fx, float delay)
    {
        yield return new WaitForSeconds(delay);
        fx.SetActive(false);
    }

    void HandleFlameGuard()
    {
        if (!hasFlameGuard || flameGuardOnCooldown) return;

        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("Flame Guard activated!");
            StartCoroutine(ActivateFlameGuard());
        }
    }

    private IEnumerator ActivateFlameGuard()
    {
        flameGuardOnCooldown = true;

        if (flameShieldObject != null)
            flameShieldObject.SetActive(true);

        float timer = 0f;

        while (timer < flameGuardDuration)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1f, enemyLayer);
            foreach (Collider2D hit in hits)
            {
                EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
                if (enemy != null)
                {
                    Vector2 knockbackDir = (hit.transform.position - transform.position).normalized;
                    enemy.TakeDamage(contactDamage, knockbackDir, 2f);
                }
            }

            timer += 0.5f;
            yield return new WaitForSeconds(0.5f);
        }

        if (flameShieldObject != null)
            flameShieldObject.SetActive(false);

        yield return new WaitForSeconds(flameGuardCooldown);
        flameGuardOnCooldown = false;
        Debug.Log("Flame Guard ready again.");
    }

    void OnDrawGizmosSelected()
    {
        if (hasCommandPulse)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, pulseRadius);
        }
    }
}
