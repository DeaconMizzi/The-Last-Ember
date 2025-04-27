using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Animator anim;
    private Vector2 moveInput;
    private Vector2 moveVelocity;

    private float lastMoveX = 0f;
    private float lastMoveY = -1f; // Facing down initially

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveVelocity = moveInput.normalized * moveSpeed;

        bool isMoving = moveInput.sqrMagnitude > 0.1f;

        if (isMoving)
        {
            lastMoveX = moveInput.x;
            lastMoveY = moveInput.y;
        }

        // Animator Parameters
        anim.SetBool("isMoving", isMoving);
        anim.SetFloat("moveX", isMoving ? moveInput.x : lastMoveX);
        anim.SetFloat("moveY", isMoving ? moveInput.y : lastMoveY);
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + moveVelocity * Time.fixedDeltaTime);
    }
}
