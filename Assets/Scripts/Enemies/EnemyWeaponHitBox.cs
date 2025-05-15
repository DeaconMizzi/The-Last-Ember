using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWeaponHitbox : MonoBehaviour
{
    public int damage = 1;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Hitbox collided with: " + collision.collider.name); // Debug line

        if (collision.collider.CompareTag("Player"))
        {
            PlayerHealth player = collision.collider.GetComponent<PlayerHealth>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }
        }
    }
}