using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWeaponHitbox : MonoBehaviour
{
    public int damage = 1;
    private bool hasHit = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;

        if (other.CompareTag("Player"))
        {
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
        hasHit = false;
    }
}
