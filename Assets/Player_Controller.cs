using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class Player_Controller : MonoBehaviour
{

    public float moveSpeed = 1f;

    public float collisionOffset = 0.05f;

    public ContactFilter2D movementFilter;

    public _Attack attack_;

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
            int count = rb.Cast(
                movementInput, // input between -1 and 1
                movementFilter,// where collision can occur on the layer
                castCollision, // found collision are stored in here
                moveSpeed * Time.fixedDeltaTime + collisionOffset); //amount to cast equates movement plust offset

            

            if (count == 0) //Detection of incoming ray for collision item ** cannot move once collided
            {
                rb.MovePosition(rb.position + movementInput * moveSpeed * Time.fixedDeltaTime);
                Debug.Log("Moving & Collision = " + castCollision);

            }
        }
    }

    void OnMove(InputValue movementValue)
    {
        movementInput = movementValue.Get<Vector2>();
    }
}
