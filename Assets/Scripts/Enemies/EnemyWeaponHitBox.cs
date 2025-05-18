using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWeaponHitbox : MonoBehaviour
{
    public int damage = 1;
    private bool hasHit = false;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (hasHit || !gameObject.activeInHierarchy)
            return;

        if (other.CompareTag("Player"))
        {
            Debug.Log("[Hitbox] 🩸 DEALING DAMAGE via TriggerStay");
            PlayerHealth player = other.GetComponent<PlayerHealth>();
            if (player != null)
            {
                player.TakeDamage(damage);
                hasHit = true;
            }
        }
    }

    public void ResetHit()
    {
        Debug.Log("[Hitbox] ✅ ResetHit()");
        hasHit = false;
    }
}
