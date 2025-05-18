using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandPulse : MonoBehaviour
{
    public float radius = 2f;
    public float stunDuration = 1f;
    public KeyCode activationKey = KeyCode.Q;

    void Update()
    {
        if (Input.GetKeyDown(activationKey))
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Enemy"))
                {
                    var enemy = hit.GetComponent<IStunnable>();
                    if (enemy != null)
                        enemy.Stun(stunDuration);
                }
            }

            // Optional: VFX/screen shake
        }
    }
}
