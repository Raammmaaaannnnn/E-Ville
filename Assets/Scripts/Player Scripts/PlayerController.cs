using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    [SerializeField] private float moveSpeed = 5f;
    
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float knockbackForce = 3f;
    public float invincibleTime = 1f;
    private bool isInvincible = false;

    // Start is called before the first frame update
    void Start()
    {

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        currentHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        AdjustPlayerFacingDirection();
        rb.velocity = moveInput * moveSpeed ;
      
    }

    
    public void Move(InputAction.CallbackContext context)
    {

        animator.SetBool("isMoving?", true);
        if(context.canceled)
        {
            animator.SetBool("isMoving?", false);
            animator.SetFloat("LastInputX", moveInput.x);
            animator.SetFloat("LastInputY", moveInput.y);
        }
        moveInput = context.ReadValue<Vector2>();
        animator.SetFloat("InputXCurrent",moveInput.x);
        animator.SetFloat("InputYCurrent",moveInput.y);
    }

    private void AdjustPlayerFacingDirection()
    {
        Vector3 mousePos = Input.mousePosition;
        Vector3 playerScreenPoint = Camera.main.WorldToScreenPoint(transform.position); 

        if(mousePos.x < playerScreenPoint.x)
        {
            spriteRenderer.flipX = true;
        }
        else
        {
            spriteRenderer.flipX = false;
        }
    }

    /// <summary>
    /// Health system
    /// 

    public void TakeDamage(float damage)
    {
        if (isInvincible) return;

        currentHealth -= damage;
        StartCoroutine(DamageFlash());

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }

        StartCoroutine(InvincibilityFrames());
    }

    private IEnumerator DamageFlash()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.15f);
        spriteRenderer.color = Color.white;
    }

    private IEnumerator InvincibilityFrames()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibleTime);
        isInvincible = false;
    }

    private void Die()
    {
        Debug.Log("Player has died!");
        animator.SetBool("isMoving?", false);
        rb.velocity = Vector2.zero;
        // Optionally, disable controls
        this.enabled = false;
        // TODO: Add respawn or Game Over logic here later
    }

    // Optional: small knockback effect when taking damage
    public void ApplyKnockback(Vector2 direction)
    {
        rb.AddForce(direction * knockbackForce, ForceMode2D.Impulse);
    }

}
