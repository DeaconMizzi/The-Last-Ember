using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAntiPush : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 lastPosition;
    private bool isBeingKnockedBack = false;
    private bool isAttacking = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        lastPosition = rb.position;
    }

    void Update()
    {
        // Check if the enemy is knocked back or attacking
        isBeingKnockedBack = CheckIfKnockedBack();
        isAttacking = CheckIfAttacking();
    }

    void LateUpdate()
    {
        // If not knocked back or attacking, reset position to avoid player pushing
        if (!isBeingKnockedBack && !isAttacking)
        {
            rb.MovePosition(lastPosition);
        }
        else
        {
            lastPosition = rb.position;
        }
    }

    bool CheckIfKnockedBack()
    {
        var aiScripts = GetComponents<MonoBehaviour>();
        foreach (var ai in aiScripts)
        {
            var type = ai.GetType();
            if (type.GetMethod("isKnockedBack") != null)
            {
                var field = type.GetField("isKnockedBack", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                    return (bool)field.GetValue(ai);
            }
        }
        return false;
    }

    bool CheckIfAttacking()
    {
        var aiScripts = GetComponents<MonoBehaviour>();
        foreach (var ai in aiScripts)
        {
            var type = ai.GetType();
            if (type.GetMethod("isAttacking") != null)
            {
                var field = type.GetField("isAttacking", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                    return (bool)field.GetValue(ai);
            }
        }
        return false;
    }
}