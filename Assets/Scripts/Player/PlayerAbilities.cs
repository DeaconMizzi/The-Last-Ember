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

    [Header("Command Pulse")]
    private bool hasCommandPulse = false;
    private bool pulseOnCooldown = false;

    [Header("Flame Guard")]
    private bool hasFlameGuard = false;

    void Update()
    {
        HandleBloomstep();
        HandleCommandPulse();
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

    // ===== Command Pulse Logic =====
    void HandleCommandPulse()
    {
        if (!hasCommandPulse || pulseOnCooldown) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("⚡ Command Pulse Activated!");
            StartCoroutine(PulseCooldown(5f));
        }
    }

    IEnumerator PulseCooldown(float duration)
    {
        pulseOnCooldown = true;
        yield return new WaitForSeconds(duration);
        pulseOnCooldown = false;
    }
}
