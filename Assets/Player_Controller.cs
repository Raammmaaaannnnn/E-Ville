using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class Player_Controller : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float collisionRadius = 0.1f;
    public LayerMask collisionLayer;
    public _Attack attack_;
    public DrunkEffect drunkEffect;

    private Vector2 movementInput;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private bool canMove = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        drunkEffect = GetComponent<DrunkEffect>();
    }

    void FixedUpdate()
    {
        if (!canMove) return;

        Vector2 moveDir = movementInput.normalized;

        if (drunkEffect != null && drunkEffect.isDrunk)
            moveDir += drunkEffect.GetDrunkMovementOffset();

        moveDir = Vector2.ClampMagnitude(moveDir, 1f);

        Vector2 targetPos = rb.position + moveDir * moveSpeed * Time.fixedDeltaTime;

        // Collision check using OverlapCircle instead of Cast
        if (!Physics2D.OverlapCircle(targetPos, collisionRadius, collisionLayer))
        {
            rb.MovePosition(targetPos);
        }

        bool isMoving = movementInput.sqrMagnitude > 0.01f;

        // Animation Handling
        animator.SetBool("isMoving?", isMoving);
        animator.SetBool("isDrunk?", drunkEffect != null && drunkEffect.isDrunk);

        // Flip sprite based on direction
        if (movementInput.x > 0.1f) spriteRenderer.flipX = false;
        else if (movementInput.x < -0.1f) spriteRenderer.flipX = true;
    }

    void OnMove(InputValue movementValue)
    {
        movementInput = movementValue.Get<Vector2>();
    }

    void OnFire()
    {
        animator.SetTrigger("Attack");
    }

    public void Attack()
    {
        LockMovement();
        if (spriteRenderer.flipX)
            attack_.AttackLeft();
        else
            attack_.AttackRight();
    }

    public void LockMovement() => canMove = false;
    public void UnlockMovement() => canMove = true;
}
