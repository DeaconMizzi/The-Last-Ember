using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GolemSlamDamage : MonoBehaviour
{
    public int damage = 2;
    public float stunDuration = 0.5f;
    public float lifeTime = 0.3f;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth player = other.GetComponent<PlayerHealth>();
            if (player != null)
            {
                player.TakeDamage(damage);
                //Calls a stun method
                PlayerMovement movement = player.GetComponent<PlayerMovement>();
                if (movement != null)
                    movement.ApplyStun(stunDuration);
            }
        }
    }
}
