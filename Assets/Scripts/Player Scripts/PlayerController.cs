using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    [SerializeField] private float moveSpeed = 5f;
    //[SerializeField] _Attack attack_Hitbox;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 lastMoveInput;
    private Animator animator;
    public Transform Attack_aim;
    private bool isMoving = true;
    private bool isAttacking = false;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        rb.velocity = moveInput * moveSpeed;
        if (isMoving) // Stop movement when attacking
        {
            Vector3 vector3 = Vector3.left * moveInput.x + Vector3.down * moveInput.y;
            Attack_aim.rotation = Quaternion.LookRotation(Vector3.forward, vector3);

        }
        
    }
   
    public void Move(InputAction.CallbackContext context)
    {
        animator.SetBool("isMoving?", true);

        if (context.canceled)
        {
            isMoving = false;
            animator.SetBool("isMoving?", false);
            //if (isAttacking) return;
            animator.SetFloat("LastInputX", moveInput.x);
            animator.SetFloat("LastInputY", moveInput.y);
            Vector3 vector3 = Vector3.left * moveInput.x + Vector3.down * moveInput.y;
            Attack_aim.rotation = Quaternion.LookRotation(Vector3.forward, vector3);
        }
        moveInput = context.ReadValue<Vector2>();
        animator.SetFloat("InputXCurrent", moveInput.x);
        animator.SetFloat("InputYCurrent", moveInput.y);


    }

    /*
    public void OnAttackInput(InputAction.CallbackContext context)
    {
        if (context.performed && !isAttacking)
        {
            isAttacking = true;
            canMove = false;
            animator.SetTrigger("Attack");

            float x = animator.GetFloat("LastInputX");
            float y = animator.GetFloat("LastInputY");

            // Direction-based attack handling
            if (Mathf.Abs(x) > Mathf.Abs(y))
            {
                attackHitBoxLeftAndRight.SetActive(true);
                if (x > 0)
                    attack_Hitbox.AttackRight();
                else
                    attack_Hitbox.AttackLeft();
            }
            else
            {
                if (y > 0)
                {
                    attackHitBoxUp.SetActive(true);
                    attack_Hitbox.AttackUp();
                }
                else
                {
                    attackHitBoxDown.SetActive(true);
                    attack_Hitbox.AttackDown();
                }
                 
            }
        }

        
        
        if (context.performed && !isAttacking)
        {
            isAttacking = true;
            animator.SetTrigger("Attack");

            // Determine direction of attack
            if (animator.GetFloat("LastInputX") < 0)
                attack_Hitbox.AttackLeft();
            else
                attack_Hitbox.AttackRight();

        }
    }

    // Called from Animation Event
    public void StopAttack()
    {
        attack_Hitbox.StopAttack();
        isAttacking = false;
        canMove = true;
    }

    */













    /////////////////////////////

    /*

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

    public void EndAttack()
    {
        UnlockMovement();   
        attack_.StopAttack();
    }

    public void LockMovement()
    {
        canMove = false;
    }

    public void UnlockMovement()
    {
        canMove = true;
    }

    */

}
