using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public bool canMove = true; // ← ADD THIS

    private Rigidbody2D rb;
    private Animator anim;
    private Vector2 moveInput;
    private Vector2 moveVelocity;
    public bool isDashing = false;
    private float lastMoveX = 0f;
    private float lastMoveY = -1f; // Facing down initially
    public bool isStunned = false;
    public Vector2 LastMoveDirection => new Vector2(lastMoveX, lastMoveY).normalized;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (!canMove)
        {
            moveVelocity = Vector2.zero;
            rb.velocity = Vector2.zero;
            moveInput = Vector2.zero;
            anim.SetBool("isMoving", false);
            return;
        }

        if (!DialogueManager.DialogueIsOpen)
        {
            moveInput.x = Input.GetAxisRaw("Horizontal");
            moveInput.y = Input.GetAxisRaw("Vertical");
            moveVelocity = moveInput.normalized * moveSpeed;
        }
        else
        {
            moveInput = Vector2.zero;
            moveVelocity = Vector2.zero;
            rb.velocity = Vector2.zero;
        }

        bool isMoving = moveInput.sqrMagnitude > 0.1f;

        if (isMoving)
        {
            lastMoveX = moveInput.x;
            lastMoveY = moveInput.y;
        }

        anim.SetBool("isMoving", isMoving);
        anim.SetFloat("moveX", isMoving ? moveInput.x : lastMoveX);
        anim.SetFloat("moveY", isMoving ? moveInput.y : lastMoveY);
    }


   void FixedUpdate()
    {
        if (canMove && !isDashing)
        {
            rb.MovePosition(rb.position + moveVelocity * Time.fixedDeltaTime);
        }
    }
    public void ApplyStun(float duration)
    {
        if (!isStunned)
            StartCoroutine(StunRoutine(duration));
    }

    IEnumerator StunRoutine(float duration)
    {
        isStunned = true;
        canMove = false;

        yield return new WaitForSeconds(duration);

        canMove = true;
        isStunned = false;
    }
}
