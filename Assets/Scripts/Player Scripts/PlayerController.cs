using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    public float moveSpeed = 1f;

    public float collisionOffset = 0.05f;

    public ContactFilter2D movementFilter;

    Vector2 movementInput;

    Rigidbody2D rb;

    SpriteRenderer spriteRenderer;

    Animator animator;

    List<RaycastHit2D> castCollision = new List<RaycastHit2D>();
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();   
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        if (movementInput != Vector2.zero) 
        {
            bool success = TryToMove(movementInput);

            if(!success)
            {
                success = TryToMove(new Vector2(movementInput.x, 0));
                if (!success)
                {
                    success = TryToMove(new Vector2(0, movementInput.y));
                }
            }

            animator.SetBool("isMoving?", success);
            

        }
        else
        {
            animator.SetBool("isMoving?", false);
        }

        if(movementInput.x > 0)
        {
            spriteRenderer.flipX = false;
        }
        else if(movementInput.x < 0)
        {
            spriteRenderer.flipX = true;
        }
        
        
    }

    private bool TryToMove(Vector2 direction)
    {
        if(direction != Vector2.zero)
        {
            int count = rb.Cast(
            direction,
            movementFilter,
            castCollision,
            moveSpeed * Time.fixedDeltaTime * collisionOffset);

        

            if (count == 0) //Detection of incoming ray for collision item ** cannot move once collided
            {
                rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime);
                Debug.Log("Moving");
                return true;
            }
            else
            {
                Debug.Log("NotMoving"); //The output once collided for various item needs to be set.
                return false;
            }
        }
        else
        {
            //no direction to move in so idle
            return false;
        }

        
    }

    void OnMove(InputValue movementValue)
    {
        movementInput = movementValue.Get<Vector2>();
    }
}
