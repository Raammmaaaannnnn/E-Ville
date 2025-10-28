using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    public float moveSpeed = 1f;

    public float collisionOffset = 0.05f;

    public ContactFilter2D movementFilter;

    public _Attack attack_;

    //drunk
    public DrunkEffect drunkEffect;

    Vector2 movementInput;

    Rigidbody2D rb;

    SpriteRenderer spriteRenderer;

    public Animator animator;

    List<RaycastHit2D> castCollision = new List<RaycastHit2D>();
    // Start is called before the first frame update

    bool canMove = true;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        //drunk
        drunkEffect = GetComponent<DrunkEffect>();
    }

    private void FixedUpdate()
    {
        if (canMove)
        {
            if (movementInput != Vector2.zero)
            {
                bool success = TryToMove(movementInput);

                if (!success)
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

            if(drunkEffect.isDrunk == true)
            {
                animator.SetBool("isDrunk?", true);
            }
            else
            {
                animator.SetBool("isDrunk?", false);
            }

            if (movementInput.x > 0)
            {
                spriteRenderer.flipX = false;
                
            }
            else if (movementInput.x < 0)
            {
                spriteRenderer.flipX = true;
                
            }
        }
        
        
        
    }

    private bool TryToMove(Vector2 direction)
    {
        if(direction != Vector2.zero)
        {
            int count = rb.Cast(
            direction, // input between -1 and 1
            movementFilter,// where collision can occur on the layer
            castCollision, // found collision are stored in here
            moveSpeed * Time.fixedDeltaTime + collisionOffset); //amount to cast equates movement plust offset

            

            if (count == 0) //Detection of incoming ray for collision item ** cannot move once collided
            {
              
                
                if (drunkEffect.isDrunk == false)
                {
                    rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime);
                    Debug.Log("Moving");

                }
                else if(drunkEffect.isDrunk == true)
                {
                    Vector2 drunkOffset = drunkEffect.GetDrunkMovementOffset();

                    // Add a little unpredictability in direction
                    Vector2 combinedMovement = (direction + drunkOffset).normalized;

                    //float drunkSpeed = moveSpeed * Mathf.Lerp(1f, 0.6f, drunkEffect.drunkenness); // slower when drunk

                    rb.MovePosition(rb.position + combinedMovement * moveSpeed * Time.fixedDeltaTime);

                    // Trigger animation
                    
                    Debug.Log("Drunk");
                }
               
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
        return true;

    }

    void OnMove(InputValue movementValue)
    {
        movementInput = movementValue.Get<Vector2>();
    }

    void OnFire()
    {
        Debug.Log("Attacking");
        animator.SetTrigger("Attack");
    }

    public void Attack()
    {
        LockMovement();
        if(spriteRenderer.flipX == true)
        {
            attack_.AttackLeft();
        }
        else
        {
            attack_.AttackRight();
        }
    }

    public void LockMovement()
    {
        canMove = false;
    }

    public void UnlockMovement()
    {
        canMove = true;
    }
}
