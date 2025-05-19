using System.Collections;
using UnityEngine;

public class PlayerAbilities : MonoBehaviour
{
    [Header("Bloomstep")]
    public GameObject flowerTrailPrefab;
    public GameObject ghostTrailPrefab;
    public Vector2 flowerSpawnOffset = new Vector2(0f, -0.5f); // align to feet
    private bool canBloomstep = false;
    private bool bloomstepOnCooldown = false;
    private CameraShaker cameraShaker;


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
    public GameObject flameShieldObject; // Assign in Inspector
    public float flameGuardDuration = 3f;
    public float flameGuardCooldown = 6f;
    public int contactDamage = 1;
    
    [Header("Pulse FX")]
    public GameObject dustFX; // Assign in Inspector (should be disabled by default)
    public Vector3 dustFXPosition = new Vector3(-0.278f, -0.901f, 0f); // Your target position


    void Start()
    {
        cameraShaker = GameObject.Find("CmCam")?.GetComponent<CameraShaker>();
        if (cameraShaker == null)
            Debug.LogWarning("CameraShaker not found on CmCam.");
    }
    void Update()
    {
        HandleBloomstep();
        HandleCommandPulse();
        HandleFlameGuard();
    }

    // ===== Ability Unlock Methods =====
    public void EnableBloomstep()
    {
        canBloomstep = true;
        Debug.Log("✅ Bloomstep unlocked!");
    }

    public void EnableFlameGuard()
    {
        hasFlameGuard = true;
        Debug.Log("✅ Flame Guard unlocked!");
    }

    public void EnableCommandPulse()
    {
        hasCommandPulse = true;
        Debug.Log("✅ Command Pulse unlocked!");
    }

    // ===== Bloomstep Logic =====
    void HandleBloomstep()
    {
        if (!canBloomstep || bloomstepOnCooldown) return;

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            Vector2 dashDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
            if (dashDirection == Vector2.zero)
                dashDirection = Vector2.right;

            float dashDistance = 2.5f;
            float dashDuration = 0.2f;

            Vector3 start = transform.position;
            Vector3 end = start + (Vector3)(dashDirection * dashDistance);

            Debug.Log("🌿 Bloomstep: smooth dash initiated.");
            StartCoroutine(SmoothDash(start, end, dashDuration));
            StartCoroutine(BloomstepCooldown(1.5f));
        }
    }

    IEnumerator SmoothDash(Vector3 start, Vector3 end, float duration)
    {
        float elapsed = 0f;
        SpriteRenderer playerSR = GetComponent<SpriteRenderer>();

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            transform.position = Vector3.Lerp(start, end, t);

            // 🎯 Randomized flower trail position (around feet)
            Vector2 randomOffset = new Vector2(
                Random.Range(-0.05f, 0.05f), // small left/right jitter
                Random.Range(-0.1f, 0.1f)    // small up/down jitter
            );
            Vector3 flowerPos = transform.position + (Vector3)flowerSpawnOffset + (Vector3)randomOffset;

            GameObject flower = Instantiate(flowerTrailPrefab, flowerPos, Quaternion.identity);

            // Force flower behind player
            SpriteRenderer flowerSR = flower.GetComponent<SpriteRenderer>();
            if (playerSR != null && flowerSR != null)
            {
                flowerSR.sortingLayerID = playerSR.sortingLayerID;
                flowerSR.sortingOrder = playerSR.sortingOrder - 2;
            }

            // 👻 Ghost sprite (only on player)
            if (ghostTrailPrefab != null)
            {
                GameObject ghost = Instantiate(ghostTrailPrefab, transform.position, Quaternion.identity);
                SpriteRenderer ghostSR = ghost.GetComponent<SpriteRenderer>();
                if (ghostSR != null && playerSR != null)
                {
                    ghostSR.sprite = playerSR.sprite;
                    ghostSR.flipX = playerSR.flipX;
                    ghostSR.sortingLayerID = playerSR.sortingLayerID;
                    ghostSR.sortingOrder = playerSR.sortingOrder - 1;
                }
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Final flower at end
        Vector2 endOffset = new Vector2(
            Random.Range(-0.05f, 0.05f),
            Random.Range(-0.1f, 0.1f)
        );
        Vector3 finalFlowerPos = end + (Vector3)flowerSpawnOffset + (Vector3)endOffset;

        GameObject finalFlower = Instantiate(flowerTrailPrefab, finalFlowerPos, Quaternion.identity);
        SpriteRenderer finalFlowerSR = finalFlower.GetComponent<SpriteRenderer>();
        if (playerSR != null && finalFlowerSR != null)
        {
            finalFlowerSR.sortingLayerID = playerSR.sortingLayerID;
            finalFlowerSR.sortingOrder = playerSR.sortingOrder - 2;
        }

        transform.position = end;
    }

    IEnumerator BloomstepCooldown(float duration)
    {
        bloomstepOnCooldown = true;
        yield return new WaitForSeconds(duration);
        bloomstepOnCooldown = false;
    }
    void HandleCommandPulse()
    {
        if (!hasCommandPulse || pulseOnCooldown)
        {
            return;
        }


        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (cameraShaker != null)
            {
                cameraShaker.Shake(6f, 0.3f); // Adjust strength & duration as needed
            }
            Debug.Log("⚡ Command Pulse Activated!");

            // 1. Optional: VFX prefab instantiate
            if (pulseVFXPrefab != null)
            {
                Instantiate(pulseVFXPrefab, transform.position, Quaternion.identity);
            }

            // 2. Enable and position dustFX if assigned
            if (dustFX != null)
            {
                dustFX.transform.position = transform.position + dustFXPosition;
                dustFX.SetActive(true);
                StartCoroutine(DisableFXAfterDelay(dustFX, 0.7f)); // Adjust delay to match animation length
            }

            // 3. Stun nearby enemies
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, pulseRadius, enemyLayer);
            foreach (Collider2D hit in hits)
            {
                IStunnable target = hit.GetComponent<IStunnable>();
                if (target != null)
                {
                    target.Stun(stunDuration);
                    Debug.Log($"⚡ Stunned: {hit.name}");
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

    void OnDrawGizmosSelected()
    {
        if (hasCommandPulse)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, pulseRadius);
        }
    }
    private IEnumerator DisableFXAfterDelay(GameObject fx, float delay)
    {
        yield return new WaitForSeconds(delay);
        fx.SetActive(false);
    }

    void HandleFlameGuard()
    {
        if (!hasFlameGuard || flameGuardOnCooldown)
            return;

        if (Input.GetKeyDown(KeyCode.R)) // You can change this key
        {
            Debug.Log("🔥 Flame Guard activated!");
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
            // Damage any enemies in contact
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
        Debug.Log("🔥 Flame Guard ready again.");
    }
}
